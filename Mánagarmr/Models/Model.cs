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

        Stream s;
        PropertyChangedEventListener listener;

        public Model()
        {
            s = new Stream();

            listener = new PropertyChangedEventListener(s);
            listener.RegisterHandler((sender, e) => UpdateHandlerProxy(sender, e));
        }

        public void Dispose()
        {
            s.Dispose();
            listener.Dispose();
        }

        private void UpdateHandlerProxy(object sender, PropertyChangedEventArgs e)
        {
            var worker = sender as Model;
            RaisePropertyChanged(e.PropertyName);
        }

        public void Play(string id, float volume)
        {            
            s.Play(id, volume);
        }

        public void Pause()
        {
            s.Pause();
        }

        public void Stop()
        {
            s.Stop();
        }

        public void ChangeVolume(float volume)
        {
            s.ChangeVolume(volume);
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

        public async void GetIndex()
        {
            await Task.Run(() =>
            {
                var gi = new GetIndexes();
                APIhelper.flipd = gi.GetIndex();
                RaisePropertyChanged("GetIndex");
            });
        }

        public async void GetLibraryList(string id)
        {
            await Task.Run(() =>
            {
                var gmd = new GetMusicDirectory();
                APIhelper.llipd = gmd.GetMusicDir(id);
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void GetRundomAlbumList()
        {
            await Task.Run(() =>
            {
                var gal = new GetAlbumList();
                APIhelper.llipd = gal.GetRandomAlbumList();
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void GetNewestAlbumList()
        {
            await Task.Run(() =>
            {
                var gal = new GetAlbumList();
                APIhelper.llipd = gal.GetNewestAlbumList();
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void Search(string query)
        {
            await Task.Run(() =>
            {
                var s2 = new Search2();
                APIhelper.llipd = s2.Search(query);
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void Tweet(string title, string artist, string album)
        {
            await Task.Run(() =>
            {
                if (String.IsNullOrEmpty(Settings.AccessToken) == false && String.IsNullOrEmpty(Settings.AccessTokenSecret) == false)
                {
                    var twitter = new Twitter();
                    twitter.Initialize(Settings.AccessToken, Settings.AccessTokenSecret);
                    twitter.Tweet(title, artist, album);
                }
            });
        }
    }
}
