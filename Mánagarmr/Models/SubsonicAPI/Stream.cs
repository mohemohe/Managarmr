using Livet;
using Livet.EventListeners;
using System;
using System.ComponentModel;
using System.Text;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class Stream : NotificationObject
    {
        private readonly PropertyChangedEventListener _listener;
        private PlayStream _ps;

        public Stream()
        {
            _ps = new PlayStream();

            _listener = new PropertyChangedEventListener(_ps);
            _listener.RegisterHandler(UpdateHandlerProxy);
        }


        private static string APIuri
        {
            get { return "rest/stream.view"; }
        }

        private string CurrentUrl { get; set; }

        public void Dispose()
        {
            _ps = null;
            _listener.Dispose();
        }

        private void UpdateHandlerProxy(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        private static string GenerateHexEncodedPassword(string basePassword)
        {
            byte[] data = Encoding.UTF8.GetBytes(basePassword);
            string hexText = BitConverter.ToString(data);
            return hexText.Replace("-", "");
        }

        public void Play(string songId, float volume)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName +
                         "&u=" + Settings.UserName + "&p=enc:" + GenerateHexEncodedPassword(Settings.Password) +
                         "&format=mp3" + "&maxBitRate=" + Settings.TargetBitrate + "&id=" + songId;

            if (url != CurrentUrl)
            {
                _ps.SetUrl(url);
                _ps.SetDevice(0);
                _ps.ChangeVolume(volume);
                _ps.PlayButton();
            }
            else
            {
                _ps.ChangeVolume(volume);
                _ps.PlayButton();
            }
        }

        public void Pause()
        {
            _ps.PausePlayback();
        }

        public void Stop()
        {
            _ps.StopPlayback();
        }

        public void ChangeVolume(float volume)
        {
            _ps.ChangeVolume(volume);
        }
    }
}