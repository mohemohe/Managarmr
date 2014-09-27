﻿using Livet;
using Livet.EventListeners;
using Mánagarmr.Models.SubsonicAPI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Mánagarmr.Models
{
    public class Model : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */

        private readonly PropertyChangedEventListener listener;
        private readonly Stream s;

        public Model()
        {
            s = new Stream();

            listener = new PropertyChangedEventListener(s);
            listener.RegisterHandler(UpdateHandlerProxy);
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
                APIhelper.Sip = gs.GetSongInfo(id);
                RaisePropertyChanged("GetSongInfo");
            });
        }

        public async void GetCoverArt(string id)
        {
            await Task.Run(() =>
            {
                var gca = new GetCoverArt();
                APIhelper.Ms = gca.GetCoverArtImage(id);
                RaisePropertyChanged("GetCoverArt");
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