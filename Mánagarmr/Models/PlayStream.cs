using Livet;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mánagarmr.Models
{
    public partial class PlayStream : NotificationObject
    {
        enum StreamingPlaybackState
        {
            Stopped,
            Playing,
            Buffering,
            Paused
        }

        public PlayStream()
        {
            timer = new System.Timers.Timer();
            timer.Interval = 250;
            timer.Elapsed += (sender, e) => timer_Tick(sender, e);
        }

        public void SetUrl(string url)
        {
            this.url = url;
        }

        public void ChangeVolume(float volume)
        {
            this.volume = volume;
            if (volumeProvider != null)
            {
                volumeProvider.Volume = volume;
            }
            Debug.WriteLine("Volume: " + volume * 100);
        }

        public string url { get; private set; }
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile StreamingPlaybackState playbackState;
        private volatile bool fullyDownloaded;
        private HttpWebRequest webRequest;
        private VolumeWaveProvider16 volumeProvider;
        private System.Timers.Timer timer;
        private float volume = 1;

        private void StreamMp3(object state)
        {
            if (Settings.IgnoreSSLcertificateError == true)
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

            fullyDownloaded = false;
            var url = (string)state;
            webRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp;
            try
            {
                resp = (HttpWebResponse)webRequest.GetResponse();
            }
            catch (WebException)
            {
                return;
            }
            var buffer = new byte[16384 * 15]; // needs to be big enough to hold a decompressed frame

            IMp3FrameDecompressor decompressor = null;
            try
            {
                using (var responseStream = resp.GetResponseStream())
                {
                    var readFullyStream = new ReadFullyStream(responseStream);
                    do
                    {
                        if (IsBufferNearlyFull)
                        {
                            Debug.WriteLine("Buffer getting full, taking a break");
                            Thread.Sleep(500);
                        }
                        else if (!fullyDownloaded)
                        {
                            Mp3Frame frame;
                            try
                            {
                                frame = Mp3Frame.LoadFromStream(readFullyStream);
                            }
                            catch (EndOfStreamException)
                            {
                                Debug.WriteLine("fullyDownloaded!");
                                fullyDownloaded = true;
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
                                bufferedWaveProvider = new BufferedWaveProvider(decompressor.OutputFormat);
                                bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(1 * 15); // allow us to get well ahead of ourselves
                            }
                            try
                            {
                                int decompressed = decompressor.DecompressFrame(frame, buffer, 0);
                                //Debug.WriteLine(String.Format("Decompressed a frame {0}", decompressed));
                                bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                            }
                            catch (ArgumentNullException)
                            {
                                Debug.WriteLine("fullyDownloaded!");
                                fullyDownloaded = true;
                                // reached the end of the MP3 file / stream
                                break;
                            }
                        }

                    } while (playbackState != StreamingPlaybackState.Stopped);
                    Debug.WriteLine("Exiting");
                    // was doing this in a finally block, but for some reason
                    // we are hanging on response stream .Dispose so never get there
                    decompressor.Dispose();
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

        private bool IsBufferNearlyFull
        {
            get
            {
                return bufferedWaveProvider != null &&
                       bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes
                       < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4;
            }
        }

        public void PlayButton()
        {
            if (playbackState == StreamingPlaybackState.Stopped)
            {
                playbackState = StreamingPlaybackState.Buffering;
                bufferedWaveProvider = null;
                ThreadPool.QueueUserWorkItem(StreamMp3, url);
                timer.Enabled = true;
            }
            else if (playbackState == StreamingPlaybackState.Paused)
            {
                playbackState = StreamingPlaybackState.Buffering;
            }
        }

        public void StopPlayback()
        {
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (!fullyDownloaded)
                {
                    webRequest.Abort();
                }

                playbackState = StreamingPlaybackState.Stopped;
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }
                timer.Enabled = false;
                // n.b. streaming thread may not yet have exited
                Thread.Sleep(500);

                RaisePropertyChanged("Stopped");
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false;
            if (playbackState != StreamingPlaybackState.Stopped)
            {
                if (waveOut == null && bufferedWaveProvider != null)
                {
                    if (IsBufferNearlyFull)
                    {
                        Debug.WriteLine("Creating WaveOut Device");
                        waveOut = CreateWaveOut();
                        volumeProvider = new VolumeWaveProvider16(bufferedWaveProvider);
                        volumeProvider.Volume = volume;
                        try
                        {
                            waveOut.Init(volumeProvider);
                        }
                        catch (NotSupportedException)
                        {
                            StopPlayback();
                            RaisePropertyChanged("WASAPI_NonSupport");
                        }
                        //progressBarBuffer.Maximum = (int)bufferedWaveProvider.BufferDuration.TotalMilliseconds;
                    }
                }
                else if (waveOut != null && bufferedWaveProvider != null)
                {
                    var bufferedSeconds = bufferedWaveProvider.BufferedDuration.TotalSeconds;
                    // make it stutter less if we buffer up a decent amount before playing
                    if (bufferedSeconds < 0.5 && playbackState == StreamingPlaybackState.Playing && !fullyDownloaded)
                    {
                        Pause();
                        RaisePropertyChanged("Paused");
                    }
                    else if (bufferedSeconds > 4 && playbackState == StreamingPlaybackState.Buffering)
                    {
                        Play();
                        RaisePropertyChanged("Playing");
                    }
                    else if (fullyDownloaded && bufferedSeconds == 0)
                    {
                        Debug.WriteLine("Reached end of stream");
                        StopPlayback();
                    }
                }

            }
            timer.Enabled = true;
        }

        private void Play()
        {
            waveOut.Play();
            Debug.WriteLine(String.Format("Started playing, waveOut.PlaybackState={0}", waveOut.PlaybackState));
            playbackState = StreamingPlaybackState.Playing;

            RaisePropertyChanged("Playing");
        }

        private void Pause()
        {
            playbackState = StreamingPlaybackState.Buffering;
            waveOut.Pause();
            Debug.WriteLine(String.Format("Paused to buffer, waveOut.PlaybackState={0}", waveOut.PlaybackState));
        }

        private IWavePlayer CreateWaveOut()
        {
            switch (Settings.AudioMethod)
            {
                case 1:
                    Debug.WriteLine("Select DirectSound");
                    return new NAudio.Wave.DirectSoundOut(Settings.AudioBuffer);
                case 2:
                    Debug.WriteLine("Select WASAPI : Shared");
                    return new NAudio.Wave.WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, true, Settings.AudioBuffer);
                case 3:
                    Debug.WriteLine("Select WASAPI : Exclusive");
                    return new NAudio.Wave.WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Exclusive, true, Settings.AudioBuffer);
                default:
                    Debug.WriteLine("Select WaveOut");
                    return new WaveOut();
            }
            
        }

        private void MP3StreamingPanel_Disposing(object sender, EventArgs e)
        {
            StopPlayback();
        }

        public void PausePlayback()
        {
            if (playbackState == StreamingPlaybackState.Playing || playbackState == StreamingPlaybackState.Buffering)
            {
                waveOut.Pause();
                Debug.WriteLine(String.Format("User requested Pause, waveOut.PlaybackState={0}", waveOut.PlaybackState));
                playbackState = StreamingPlaybackState.Paused;
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            StopPlayback();
        }
    }
}
