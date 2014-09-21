using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using Mánagarmr.Models;
using System.Drawing;
using Mánagarmr.Views;
using System.Diagnostics;
using System.Windows.Controls;
using Mánagarmr.Models.SubsonicAPI;
using System.Threading.Tasks;
using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;
using System.IO;
using System.Threading;

namespace Mánagarmr.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        Model model;
        Stopwatch sw;
        System.Timers.Timer timer;
        State currentState;

        public async void Initialize()
        {
            if (Settings.Initialize() == false)
            {
                ConfigWindowOpen();
            }
            model = new Model(); //DEBUG
            sw = new Stopwatch();

            var listener = new PropertyChangedEventListener(model);
            listener.RegisterHandler((sender, e) => UpdateHandler(sender, e));
            this.CompositeDisposable.Add(listener);

            timer = new System.Timers.Timer();
            timer.Interval = 50;
            timer.Elapsed += (sender, e) => Timer_Tick();
            timer.Start();

            currentState = State.Stopped;

            Volume = Settings.Volume;
            VolumeString = Convert.ToInt32(Volume * 100).ToString() + " %";

            await Task.Run(() => APIhelper.BuildBaseUrl());
        }

        public new void Dispose()
        {
            model.Dispose();
            model = null;
            sw = null;
            timer = null;

            Settings.Volume = Volume;
            Settings.WriteSettings();
        }

        private void UpdateHandler(object sender, PropertyChangedEventArgs e)
        {
            var worker = sender as Model;
            if (e.PropertyName == "Playing")
            {
                sw.Start();
                currentState = State.Playing;
            }
            if (e.PropertyName == "Stopped")
            {
                sw.Reset();
                currentState = State.Stopped;
                SetPlayIcon();
            }
            if (e.PropertyName == "ReadyToCoverArt")
            {
                CoverArt = _CoverArt;
            }
            if (e.PropertyName == "GetSongInfo")
            {
                SetSongInfo();
            }
            if (e.PropertyName == "GetCoverArt")
            {
                SetCoverArt();
            }
        }

        private void Timer_Tick()
        {
            ProgresBarIsEnabled = true;
            if (sw != null)
            {
                ChangeProgressBarCurrentValue(sw.ElapsedMilliseconds);
            }
            ProgresBarIsEnabled = false;
        }

        enum State
        {
            Playing,
            Paused,
            Stopped
        }


        #region WindowColor変更通知プロパティ
        private string _WindowColor = "#FF41B1E1";

        public string WindowColor
        {
            get
            { return _WindowColor; }
            set
            { 
                if (_WindowColor == value)
                    return;
                _WindowColor = value;
                RaisePropertyChanged();
            }
        }

        public void ActivatedWindow()
        {
            WindowColor = "#FF41B1E1";
        }

        public void DeactivatedWindow()
        {
            WindowColor = "#FF808080";
        }
        #endregion

        #region Title変更通知プロパティ
        private string _Title;

        public string Title
        {
            get
            { return _Title; }
            set
            { 
                if (_Title == value)
                    return;
                _Title = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Artist変更通知プロパティ
        private string _Artist;

        public string Artist
        {
            get
            { return _Artist; }
            set
            { 
                if (_Artist == value)
                    return;
                _Artist = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AlbumTitle変更通知プロパティ
        private string _AlbumTitle;

        public string AlbumTitle
        {
            get
            { return _AlbumTitle; }
            set
            { 
                if (_AlbumTitle == value)
                    return;
                _AlbumTitle = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AlbumETC変更通知プロパティ
        private string _AlbumETC;

        public string AlbumETC
        {
            get
            { return _AlbumETC; }
            set
            { 
                if (_AlbumETC == value)
                    return;
                _AlbumETC = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region CoverArt変更通知プロパティ
        private BitmapSource _CoverArt;

        public BitmapSource CoverArt
        {
            get
            { return _CoverArt; }
            set
            { 
                _CoverArt = value;
                RaisePropertyChanged();
            }
        }

        private void SetCoverArt(BitmapSource coverArt)
        {
            CoverArt = coverArt;
        }
        #endregion

        #region ProgresBarIsEnabled変更通知プロパティ
        private bool _ProgresBarIsEnabled = false;

        public bool ProgresBarIsEnabled
        {
            get
            { return _ProgresBarIsEnabled; }
            set
            { 
                if (_ProgresBarIsEnabled == value)
                    return;
                _ProgresBarIsEnabled = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ProgressBarMaxValue変更通知プロパティ
        private ulong _ProgressBarMaxValue = 100;

        public ulong ProgressBarMaxValue
        {
            get
            { return _ProgressBarMaxValue; }
            set
            { 
                if (_ProgressBarMaxValue == value)
                    return;
                _ProgressBarMaxValue = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ProgressBarCurrentValue変更通知プロパティ
        private double _ProgressBarCurrentValue = 0;

        public double ProgressBarCurrentValue
        {
            get
            { return _ProgressBarCurrentValue; }
            set
            { 
                if (_ProgressBarCurrentValue == value)
                    return;
                _ProgressBarCurrentValue = value;
                RaisePropertyChanged();
            }
        }

        private void ChangeProgressBarCurrentValue(double value)
        {
            ProgressBarCurrentValue = value;
        }

        #endregion

        #region Volume変更通知プロパティ
        private float _Volume = 0.5F;

        public float Volume
        {
            get
            { return _Volume; }
            set
            { 
                if (_Volume == value)
                    return;
                _Volume = value;
                model.ChangeVolume(value);
                VolumeString = Convert.ToInt32(value * 100).ToString() + " %";
                RaisePropertyChanged();
            }
        }
        #endregion

        #region VolumeString変更通知プロパティ
        private string _VolumeString;

        public string VolumeString
        {
            get
            { return _VolumeString; }
            set
            { 
                if (_VolumeString == value)
                    return;
                _VolumeString = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ConfigWindowOpenCommand
        private ViewModelCommand _ConfigWindowOpenCommand;

        public ViewModelCommand ConfigWindowOpenCommand
        {
            get
            {
                if (_ConfigWindowOpenCommand == null)
                {
                    _ConfigWindowOpenCommand = new ViewModelCommand(ConfigWindowOpen);
                }
                return _ConfigWindowOpenCommand;
            }
        }

        public void ConfigWindowOpen()
        {
            var cw = new ConfigWindow();
            cw.ShowDialog();
        }
        #endregion

        #region TweetCommand
        private ViewModelCommand _TweetCommand;

        public ViewModelCommand TweetCommand
        {
            get
            {
                if (_TweetCommand == null)
                {
                    _TweetCommand = new ViewModelCommand(Tweet);
                }
                return _TweetCommand;
            }
        }

        public void Tweet()
        {
            model.Tweet(Title, Artist, AlbumTitle);
        }
        #endregion

        #region PlayPauseIcon変更通知プロパティ
        private Canvas _PlayPauseIcon = (Canvas)App.Current.Resources["appbar_control_play"];

        public Canvas PlayPauseIcon
        {
            get
            { return _PlayPauseIcon; }
            set
            { 
                if (_PlayPauseIcon == value)
                    return;
                _PlayPauseIcon = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PlayPauseIconMargin変更通知プロパティ
        private string _PlayPauseIconMargin = "4,0,0,0";

        public string PlayPauseIconMargin
        {
            get
            { return _PlayPauseIconMargin; }
            set
            { 
                if (_PlayPauseIconMargin == value)
                    return;
                _PlayPauseIconMargin = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PlayPauseIconSize変更通知プロパティ
        private int _PlayPauseIconSize = 24;

        public int PlayPauseIconSize
        {
            get
            { return _PlayPauseIconSize; }
            set
            { 
                if (_PlayPauseIconSize == value)
                    return;
                _PlayPauseIconSize = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PlayPauseCommand
        private ViewModelCommand _PlayPauseCommand;

        public ViewModelCommand PlayPauseCommand
        {
            get
            {
                if (_PlayPauseCommand == null)
                {
                    _PlayPauseCommand = new ViewModelCommand(PlayPause);
                }
                return _PlayPauseCommand;
            }
        }

        public void PlayPause()
        {
            if (currentState == State.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public async void Play()
        {
            //TODO: AudioID
            string id = "10220";

            model.Play(id, Volume); //DEBUG
            SetPauseIcon();

            if (currentState != State.Paused)
            {
                model.GetSongInfo(id);
                await Task.Run(() => 
                {
                    Thread.Sleep(1000);
                    model.GetCoverArt(id);
                });
            }
            currentState = State.Playing;
        }

        public void Pause()
        {
            sw.Stop();
            model.Pause();
            SetPlayIcon();
            currentState = State.Paused;
        }

        public void SetPlayIcon()
        {
            PlayPauseIcon = (Canvas)App.Current.Resources["appbar_control_play"];
            PlayPauseIconMargin = "4,0,0,0";
            PlayPauseIconSize = 24;
        }

        public void SetPauseIcon()
        {
            PlayPauseIcon = (Canvas)App.Current.Resources["appbar_control_pause"];
            PlayPauseIconMargin = "0,0,0,0";
            PlayPauseIconSize = 20;
        }

        public void SetSongInfo()
        {
            var sip = APIhelper.sip;
            if (sip != null && sip.status != "ok")
            {
                return;
            }

            ProgressBarMaxValue = Convert.ToUInt64(sip.duration) * 1000;

            if (sip.title.Length > 30)
            {
                Title = sip.title.Substring(0, 30) + "...";
            }
            else
            {
                Title = sip.title;
            }

            string artist;
            string album;
            if (sip.artist.Length > 25)
            {
                artist = sip.artist.Substring(0, 25) + "...";
            }
            else
            {
                artist = sip.artist;
            }
            if (sip.album.Length > 25)
            {
                album = sip.album.Substring(0, 25) + "...";
            }
            else
            {
                album = sip.album;
            }
            Artist = artist;
            AlbumTitle = album;
            AlbumETC = artist + " / " + album;
        }

        public void SetCoverArt()
        {
            var coverArt = ConvertBitmap(APIhelper.ms);
            coverArt.Freeze();
            CoverArt = coverArt;
        }

        private BitmapSource ConvertBitmap(MemoryStream ms)
        {
            var bitmap = new Bitmap(ms);
            BitmapSource bs = null;

            try
            {
                var hBitmap = bitmap.GetHbitmap();

                bs = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch { }

            return bs;
        }
        #endregion

        #region StopCommand
        private ViewModelCommand _StopCommand;

        public ViewModelCommand StopCommand
        {
            get
            {
                if (_StopCommand == null)
                {
                    _StopCommand = new ViewModelCommand(Stop);
                }
                return _StopCommand;
            }
        }

        public void Stop()
        {
            model.Stop();
            sw.Reset();
            currentState = State.Stopped;
            PlayPauseIcon = (Canvas)App.Current.Resources["appbar_control_play"];
            PlayPauseIconMargin = "4,0,0,0";
            PlayPauseIconSize = 24;
        }
        #endregion

        #region RewindCommand
        private ViewModelCommand _RewindCommand;

        public ViewModelCommand RewindCommand
        {
            get
            {
                if (_RewindCommand == null)
                {
                    _RewindCommand = new ViewModelCommand(Rewind);
                }
                return _RewindCommand;
            }
        }

        public void Rewind()
        {

        }
        #endregion

        #region FastForwardCommand
        private ViewModelCommand _FastForwardCommand;

        public ViewModelCommand FastForwardCommand
        {
            get
            {
                if (_FastForwardCommand == null)
                {
                    _FastForwardCommand = new ViewModelCommand(FastForward);
                }
                return _FastForwardCommand;
            }
        }

        public void FastForward()
        {

        }
        #endregion

    }
}
