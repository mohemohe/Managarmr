using Livet;
using Livet.EventListeners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mánagarmr.Models.SubsonicAPI
{
    class Stream : NotificationObject
    {
        PlayStream ps;
        PropertyChangedEventListener listener;
        private string APIuri { get { return "rest/stream.view"; } }
        private string currentUrl { get; set; }

        public Stream()
        {
            ps = new PlayStream();

            listener = new PropertyChangedEventListener(ps);
            listener.RegisterHandler((sender, e) => UpdateHandlerProxy(sender, e));
        }

        public void Dispose()
        {
            ps = null;
            listener.Dispose();
        }

        private void UpdateHandlerProxy(object sender, PropertyChangedEventArgs e)
        {
            var worker = sender as Model;
            RaisePropertyChanged(e.PropertyName);
        }

        private string GenerateHexEncodedPassword(string basePassword)
        {
            var data = Encoding.UTF8.GetBytes(Settings.Password);
            var hexText = BitConverter.ToString(data);
            return hexText.Replace("-", "");
        }

        public void Play(string songId, float volume)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName +
                      "&u=" + Settings.UserName + "&p=enc:" + GenerateHexEncodedPassword(Settings.Password) + "&id=" + songId;

            if (url != currentUrl)
            {
                ps.SetUrl(url);
                ps.ChangeVolume(volume);
                ps.PlayButton();
            }
            else
            {
                ps.ChangeVolume(volume);
                ps.PlayButton();
            }
        }

        public void Pause()
        {
            ps.PausePlayback();
        }

        public void Stop()
        {
            ps.StopPlayback();
        }

        public void ChangeVolume(float volume)
        {
            ps.ChangeVolume(volume);
        }

    }
}
