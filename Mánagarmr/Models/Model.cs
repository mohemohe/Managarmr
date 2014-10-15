using System.Diagnostics;
using Livet;
using Livet.EventListeners;
using Mánagarmr.Models.SubsonicAPI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using NAudio.Wave;

namespace Mánagarmr.Models
{
    public class Model : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        private readonly PropertyChangedEventListener _listener;
        private readonly Stream _s;

        public Model()
        {
            _s = new Stream();

            _listener = new PropertyChangedEventListener(_s);
            _listener.RegisterHandler(UpdateHandlerProxy);
        }

        public void Dispose()
        {
            _s.Dispose();
            _listener.Dispose();
        }

        private void UpdateHandlerProxy(object sender, PropertyChangedEventArgs e)
        {
// ReSharper disable once UnusedVariable
            var worker = sender as Model;
            RaisePropertyChanged(e.PropertyName);
        }

        public void Play(string songid, float volume)
        {
            _s.Play(songid, volume);
        }

        public void Pause()
        {
            _s.Pause();
        }

        public void Stop()
        {
            _s.Stop();
        }

        public void ChangeVolume(float volume)
        {
            _s.ChangeVolume(volume);
        }

        public async void GetSongInfo(string id)
        {
            await Task.Run(() =>
            {
                var gs = new GetSong();
                APIhelper.Sip = gs.GetSongInfo(id);
                RaisePropertyChanged("GetSongInfo");
            });
        }

        public async void GetIndex()
        {
            await Task.Run(() =>
            {
                var gi = new GetIndexes();
                APIhelper.Flipd = gi.GetIndex();
                RaisePropertyChanged("GetIndex");
            });
        }

        public async void GetFolderName(string id)
        {
            await Task.Run(() =>
            {
                var gmd = new GetMusicDirectory();
                APIhelper.FolderName = gmd.GetMusicDirName(id);
                RaisePropertyChanged("GetFolderName");
            });
        }

        public async void GetLibraryList(string id)
        {
            await Task.Run(() =>
            {
                var gmd = new GetMusicDirectory();
                APIhelper.Llipd = gmd.GetMusicDir(id);
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void GetRundomAlbumList()
        {
            await Task.Run(() =>
            {
                var gal = new GetAlbumList();
                APIhelper.Llipd = gal.GetRandomAlbumList();
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void GetNewestAlbumList()
        {
            await Task.Run(() =>
            {
                var gal = new GetAlbumList();
                APIhelper.Llipd = gal.GetNewestAlbumList();
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void GetLibraryListHeader(string id)
        {
            await Task.Run(() =>
            {
                var ga = new GetAlbum();
                APIhelper.Header = ga.GetAlbumInfo(id);
                RaisePropertyChanged("GetLibraryListHeader");
            });
        }

        public async void Search(string query)
        {
            await Task.Run(() =>
            {
                var s2 = new Search2();
                APIhelper.Llipd = s2.Search(query);
                RaisePropertyChanged("GetLibraryList");
            });
        }

        public async void Tweet(string title, string artist, string album)
        {
            await Task.Run(() =>
            {
                if (String.IsNullOrEmpty(Settings.AccessToken) == false &&
                    String.IsNullOrEmpty(Settings.AccessTokenSecret) == false)
                {
                    var twitter = new Twitter();
                    twitter.Initialize(Settings.AccessToken, Settings.AccessTokenSecret);
                    twitter.Tweet(title, artist, album);
                }
            });
        }
    }
}