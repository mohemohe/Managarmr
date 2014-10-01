using System.CodeDom;
using System.Linq;
using System.Security.Cryptography;
using Livet;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Timer = System.Timers.Timer;

namespace Mánagarmr.Models
{
    public class PlayStream : NotificationObject
    {
        private readonly Timer _timer;
        private BufferedWaveProvider _bufferedWaveProvider;
        private volatile bool _fullyDownloaded;
        private volatile StreamingPlaybackState _playbackState;
        private float _volume = 1;
        private VolumeWaveProvider16 _volumeProvider;
        private IWavePlayer _waveOut;
        private HttpWebRequest _webRequest;

        public PlayStream()
        {
            _timer = new Timer {Interval = 250};
            _timer.Elapsed += timer_Tick;
        }

        public string Url { get; private set; }
        public int DeviceID { get; private set; }

        private bool IsBufferNearlyFull
        {
            get
            {
                return _bufferedWaveProvider != null &&
                       _bufferedWaveProvider.BufferLength - _bufferedWaveProvider.BufferedBytes
                       < _bufferedWaveProvider.WaveFormat.AverageBytesPerSecond/4;
            }
        }

        public void SetUrl(string url)
        {
            Url = url;
        }

        public void SetDevice(int id)
        {
            DeviceID = id;
        }

        public void ChangeVolume(float volume)
        {
            _volume = volume;
            if (_volumeProvider != null)
            {
                _volumeProvider.Volume = volume;
            }
            Debug.WriteLine("Volume: " + volume*100);
        }

        private void StreamMp3(object state)
        {
            if (Settings.IgnoreSSLcertificateError)
            {
                if (ServicePointManager.ServerCertificateValidationCallback == null)
                {
                    ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
                }
                else
                {
                    ServicePointManager.ServerCertificateValidationCallback = null;
                }
            }

            _fullyDownloaded = false;
            var url = (string) state;
            _webRequest = (HttpWebRequest) WebRequest.Create(url);
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse) _webRequest.GetResponse();
            }
            catch (WebException)
            {
                return;
            }
            var buffer = new byte[16384*Settings.NetworkBuffer]; // needs to be big enough to hold a decompressed frame

            IMp3FrameDecompressor decompressor = null;
            try
            {
                using (Stream responseStream = resp.GetResponseStream())
                {
                    var readFullyStream = new ReadFullyStream(responseStream);
                    do
                    {
                        if (IsBufferNearlyFull)
                        {
                            Debug.WriteLine("Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else if (!_fullyDownloaded)
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                            }
                            catch (EndOfStreamException)
                            {
                                Debug.WriteLine("fullyDownloaded!");
                                _fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                            catch (WebException)
                            {
                                Debug.WriteLine("WebException!");
                                // probably we have aborted download from the GUI thread
                                break;
                            }
                            if (decompressor == null)
                            {
                                // don't think these details matter too much - just help ACM select the right codec
                                // however, the buffered provider doesn't know what sample rate it is working at
                                // until we have a frame
                                decompressor = CreateFrameDecompressor(frame);
                                _bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                _bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(Settings.NetworkBuffer);
                                // allow us to get well ahead of ourselves
                            }
                            try
                            {
                                int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                                //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                                _bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                            }
                            catch (ArgumentNullException)
                            {
                                Debug.WriteLine("fullyDownloaded!");
                                _fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                        }
                    } while (_playbackState != StreamingPlaybackState.Stopped);
                    Debug.WriteLine("Exiting");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    if (decompressor != null) decompressor.Dispose();
                }
            }
            finally
            {
                if (decompressor != null)
                {
                    decompressor.Dispose();
                }
                //StopPlayback();
            }
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        public void PlayButton()
        {
            if (_playbackState == StreamingPlaybackState.Stopped)
            {
                _playbackState = StreamingPlaybackState.Buffering;
                _bufferedWaveProvider = null;
                ThreadPool.QueueUserWorkItem(StreamMp3, Url);
                _timer.Enabled = true;
            }
            else if (_playbackState == StreamingPlaybackState.Paused)
            {
                _playbackState = StreamingPlaybackState.Buffering;
            }
        }

        public void StopPlayback()
        {
            if (_playbackState != StreamingPlaybackState.Stopped)
            {
                if (!_fullyDownloaded)
                {
                    _webRequest.Abort();
                }

                _playbackState = StreamingPlaybackState.Stopped;
                if (_waveOut != null)
                {
                    _waveOut.Stop();
                    _waveOut.Dispose();
                    _waveOut = null;
                }
                _timer.Enabled = false;
                // n.b. streaming thread may not yet have exited
                Thread.Sleep(500);

                RaisePropertyChanged("Stopped");
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            _timer.Enabled = false;
            if (_playbackState != StreamingPlaybackState.Stopped)
            {
                if (_waveOut == null && _bufferedWaveProvider != null)
                {
                    if (IsBufferNearlyFull)
                    {
                        Debug.WriteLine("Creating WaveOut Device");
                        _waveOut = CreateWaveOut();
                        _volumeProvider = new VolumeWaveProvider16(_bufferedWaveProvider);
                        _volumeProvider.Volume = _volume;
                        try
                        {
                            _waveOut.Init(_volumeProvider);
                        }
                        catch (NotSupportedException)
                        {
                            StopPlayback();
                            RaisePropertyChanged("WASAPI_NonSupport");
                        }
                        //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
                    }
                }
                else if (_waveOut != null && _bufferedWaveProvider != null)
                {
                    double bufferedSeconds = _bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && _playbackState == StreamingPlaybackState.Playing && !_fullyDownloaded)
                    {
                        Pause();
                        RaisePropertyChanged("Paused");
                    }
                    else if (bufferedSeconds > 4 && _playbackState == StreamingPlaybackState.Buffering)
                    {
                        Play();
                        RaisePropertyChanged("Playing");
                    }
                    else if (_fullyDownloaded && bufferedSeconds == 0)
                    {
                        Debug.WriteLine("Reached end of stream");
                        StopPlayback();
                    }
                }
            }
            _timer.Enabled = true;
        }

        private void Play()
        {
            _waveOut.Play();
            Debug.WriteLine("Started playing, waveOut.PlaybackState={0}", _waveOut.PlaybackState);
            _playbackState = StreamingPlaybackState.Playing;

            RaisePropertyChanged("Playing");
        }

        private void Pause()
        {
            _playbackState = StreamingPlaybackState.Buffering;
            _waveOut.Pause();
            Debug.WriteLine("Paused to buffer, waveOut.PlaybackState={0}", _waveOut.PlaybackState);
        }

        private IWavePlayer CreateWaveOut()
        {
            switch (Settings.AudioMethod)
            {
                case 0:
                    // WaveOut
                    Debug.WriteLine("Select WaveOut");

                    int deviceId = -1;
                    var dc = WaveOut.DeviceCount;
                    for (int i = 0; i < dc; i++)
                    {
                        var gc = WaveOut.GetCapabilities(i);
                        if (gc.ProductName == Settings.AudioDevice)
                        {
                            deviceId = i;
                        }
                    }
                    return new WaveOut {DeviceNumber = deviceId, DesiredLatency = Settings.AudioBuffer};

                case 1:
                    // DirectSound
                    Debug.WriteLine("Select DirectSound");
                    foreach (var device in DirectSoundOut.Devices)
                    {
                        if (device.Description == Settings.AudioDevice)
                        {
                            return new DirectSoundOut(device.Guid, Settings.AudioBuffer);
                        }
                    }
                    return new DirectSoundOut(Settings.AudioBuffer);

                case 2:
                    // WASAPI Shared
                    Debug.WriteLine("Select WASAPI : Shared");
                    return new WasapiOut(AudioClientShareMode.Shared, true, Settings.AudioBuffer);

                case 3:
                    // WASAPI Exclusive
                    Debug.WriteLine("Select WASAPI : Exclusive");
                    return new WasapiOut(AudioClientShareMode.Exclusive, true, Settings.AudioBuffer);
                
                case 4:
                    // ASIO
                    Debug.WriteLine("ASIO");
                    if (AsioOut.GetDriverNames().Any(driverName => driverName == Settings.AudioDevice))
                    {
                        return new AsioOut(Settings.AudioDevice);
                    }
                    return new AsioOut();

                default:
                    // ありえない
                    return null;
            }
        }

        public void PausePlayback()
        {
            if (_playbackState == StreamingPlaybackState.Playing || _playbackState == StreamingPlaybackState.Buffering)
            {
                _waveOut.Pause();
                Debug.WriteLine("User requested Pause, waveOut.PlaybackState={0}", _waveOut.PlaybackState);
                _playbackState = StreamingPlaybackState.Paused;
            }
        }

        private enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }
    }
}