using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Mánagarmr.Models.SubsonicAPI;
using Livet.EventListeners;
using System.ComponentModel;
using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;


namespace Mánagarmr.Models
{
    public class Model : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        SubsonicStream ss;
        PropertyChangedEventListener listener;

        public Model()
        {
            ss = new SubsonicStream();

            listener = new PropertyChangedEventListener(ss);
            listener.RegisterHandler((sender, e) => UpdateHandlerProxy(sender, e));
        }

        public void Dispose()
        {
            ss = null;
            listener.Dispose();
        }

        private void UpdateHandlerProxy(object sender, PropertyChangedEventArgs e)
        {
            var worker = sender as Model;
            RaisePropertyChanged(e.PropertyName);
        }

        public void Play(string id, float volume)
        {
            byte[] data = Encoding.UTF8.GetBytes("midas6422");
            string hexText = BitConverter.ToString(data);
            string pass = hexText.Replace("-", "");
            var url = "http://172.16.0.2:4040/rest/stream.view?u=admin&p=enc:" + pass + "&v=1.1.0&c=Managarmr&id=" + id;
            ss.SetUrl(url);
            ss.ChangeVolume(volume);
            ss.PlayButton();
        }

        public void Pause()
        {
            ss.PausePlayback();
        }

        public void Stop()
        {
            ss.StopPlayback();
        }

        public void ChangeVolume(float volume)
        {
            ss.ChangeVolume(volume);
        }

        public async void GetSongInfo(string id)
        {
            await Task.Run(() => 
            {
                var gs = new GetSong();
                APIhelper.sip = gs.GetSongInfo(id);
                RaisePropertyChanged("GetSongInfo");
            });
        }

        public async void GetCoverArt(string id)
        {
            await Task.Run(() =>
            {
                var gca = new GetCoverArt();
                APIhelper.ms = gca.GetCoverArtImage(id);
                RaisePropertyChanged("GetCoverArt");
            });
        }

        public void Tweet(string title, string artist, string album)
        {
            var twitter = new Twitter();
            twitter.ReInitialize("","");
            twitter.Tweet(title, artist, album);
        }
    }
}
