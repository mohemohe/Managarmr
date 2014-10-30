using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Mánagarmr.Models;
using Mánagarmr.Models.SubsonicAPI;
using Mánagarmr.Models.SubsonicAPI.InfoPack;
using Mánagarmr.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Timer = System.Timers.Timer;

namespace Mánagarmr.ViewModels.MainWindow
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

        private Model _model;
        private Stopwatch _sw;
        private Timer _timer;

        public async void Initialize()
        {
            _model = new Model();
            _sw = new Stopwatch();

            var listener = new PropertyChangedEventListener(_model);
            listener.RegisterHandler(UpdateHandler);
            CompositeDisposable.Add(listener);

            _timer = new Timer {Interval = 100};
            _timer.Elapsed += (sender, e) => Timer_Tick();
            _timer.Start();

            PlayState = State.Stopped;

            Volume = Settings.Volume;
            VolumeString = Convert.ToInt32(Volume*100) + " %";

            CurrentRepeatState = (RepeatState)Settings.RepeatState;

            LibraryListHeaderImage = null;
            LibraryListHeaderTitle = null;
            LibraryListHeaderArtist = null;

            await Task.Run(() => _model.GetIndex());
        }

        public void Disposes()
        {
            Stop();
            _model.Dispose();
            _model = null;
            _sw = null;
            _timer = null;

            Settings.RepeatState = (int)CurrentRepeatState;
            Settings.Volume = Volume;
            Settings.WriteSettings();
        }

        private void UpdateHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Playing":
                    ProgressBarIsIndeterminate = false;
                    _sw.Start();
                    PlayState = State.Playing;
                    SetPauseIcon();
                    break;

                case "Paused":
                    _sw.Stop();
                    PlayState = State.Paused;
                    SetPlayIcon();
                    break;

                case "Stopped":
                    _sw.Reset();
                    PlayState = State.Stopped;
                    SetPlayIcon();
                    break;

                case "GetSongInfo":
                    SetSongInfo();
                    break;

                case "GetIndex":
                    SetMusicFolder();
                    break;

                case "GetLibraryList":
                    SetLibraryList();
                    break;

                case "GetFolderName":
                    SetLibraryListHeaderTitle();
                    break;

                case "GetLibraryListHeader":
                    SetLibraryListHeader();
                    break;

                case "WASAPI_NonSupport":
                    Messenger.Raise(new InformationMessage("デバイスが対応していません", "WASAPI error", "Information"));
                    break;
            }
        }

        private void Timer_Tick()
        {
            if (_sw != null)
            {
                ChangeProgressBarCurrentValue(_sw.ElapsedMilliseconds);
            }
        }

        public void ChangeLibraryList()
        {
            LibraryListHeaderTitle = MusicFolderListValue;
            _model.GetLibraryList(GetMusicFolderListId(MusicFolderListIndex));
        }

        public void MoveLibraryList()
        {
            string id = GetLibraryListId(LibraryListIndex);
            string headerTitle = _LibraryList[LibraryListIndex].Title;
            if (IsDir(LibraryListIndex))
            {
                if (headerTitle == "...")
                {
                    _model.GetFolderName(id);
                }
                else
                {
                    LibraryListHeaderTitle = headerTitle;
                }
                
                _model.GetLibraryList(id);
            }
            else
            {
                PlayList = new List<LibraryList>();
                PlayList = new List<LibraryList>
                {
                    new LibraryList
                    {
                        ID = LibraryList[LibraryListIndex].ID,
                        Track = String.Format("{0, 5}", (PlayList.Count + 1) + ". "),
                        Artist = LibraryList[LibraryListIndex].Artist,
                        Title = LibraryList[LibraryListIndex].Title
                    }
                };
                PlayListIndex = 0;
                PlayId = PlayList[PlayListIndex].ID;
                if (PlayState != State.Stopped) Stop();
                Play();
            }
        }

        public void MovePlayList()
        {
            PlayId = PlayList[PlayListIndex].ID;
            if (PlayState != State.Stopped) Stop();
            Play();
        }

        public void GetRandomAlbumList()
        {
            LibraryListHeaderArtist = null;
            LibraryListHeaderImage = null;
            LibraryListHeaderTitle = "Random";

            _model.GetRundomAlbumList();
        }

        public void GetNewestAlbumList()
        {
            LibraryListHeaderArtist = null;
            LibraryListHeaderImage = null;
            LibraryListHeaderTitle = "Recently added";

            _model.GetNewestAlbumList();
        }

        public void SetLibraryListHeaderTitle()
        {
            LibraryListHeaderTitle = APIhelper.FolderName;
        }

        #region WindowColor変更通知プロパティ

        private string _WindowColor = "#FF41B1E1";

        public string WindowColor
        {
            get { return _WindowColor; }
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

        #endregion WindowColor変更通知プロパティ

        #region SearchQuery変更通知プロパティ

        private string _SearchQuery;

        public string SearchQuery
        {
            get { return _SearchQuery; }
            set
            {
                if (_SearchQuery == value)
                    return;
                _SearchQuery = value;
                RaisePropertyChanged();

                if (!String.IsNullOrEmpty(value))
                {
                    LibraryListHeaderTitle = "Search: " + value;
                    LibraryListHeaderArtist = null;
                    LibraryListHeaderImage = null;
                    Search();
                }
                else
                {
                    LibraryListHeaderTitle = null;
                    LibraryListHeaderArtist = null;
                    LibraryListHeaderImage = null;
                }
            }
        }

        #endregion SearchQuery変更通知プロパティ

        #region MusicFolderList変更通知プロパティ

        private List<string> _MusicFolderList;

        public List<string> MusicFolderList
        {
            get { return _MusicFolderList; }
            set
            {
                if (_MusicFolderList == value)
                    return;
                _MusicFolderList = value;
                RaisePropertyChanged();
            }
        }

        #endregion MusicFolderList変更通知プロパティ

        #region MusicFolderListId変更通知プロパティ

        private string _MusicFolderListId;

        public string MusicFolderListId
        {
            get { return _MusicFolderListId; }
            set
            {
                _MusicFolderListId = value;
                RaisePropertyChanged();
            }
        }

        #endregion MusicFolderListId変更通知プロパティ

        #region MusicFolderListValue変更通知プロパティ

        private string _MusicFolderListValue;

        public string MusicFolderListValue
        {
            get { return _MusicFolderListValue; }
            set
            {
                if (_MusicFolderListValue == value)
                    return;
                _MusicFolderListValue = value;
                RaisePropertyChanged();
            }
        }

        #endregion MusicFolderListValue変更通知プロパティ

        #region MusicFolderListIndex変更通知プロパティ

        private int _MusicFolderListIndex = -1;

        public int MusicFolderListIndex
        {
            get { return _MusicFolderListIndex; }
            set
            {
                _MusicFolderListIndex = value;
                RaisePropertyChanged();
            }
        }

        private string GetMusicFolderListId(int index)
        {
            var flip = new List<FolderListInfoPack>(APIhelper.Flipd.Values);

            if (index >= 0 && index < flip.Count)
            {
                return flip[index].Id;
            }
            return null;
        }

        #endregion MusicFolderListIndex変更通知プロパティ

        #region LibraryListId変更通知プロパティ

        private string _LibraryListId;

        public string LibraryListId
        {
            get { return _LibraryListId; }
            set
            {
                _LibraryListId = value;
                RaisePropertyChanged();
            }
        }

        private bool IsDir(int index)
        {
            var flip = new List<LibraryListInfoPack>(APIhelper.Llipd.Values);

            if (index >= 0 && index < flip.Count)
            {
                return flip[index].IsDir;
            }
            return true;
        }

        #endregion LibraryListId変更通知プロパティ

        #region LibraryListIndex変更通知プロパティ

        private int _LibraryListIndex = -1;

        public int LibraryListIndex
        {
            get { return _LibraryListIndex; }
            set
            {
                _LibraryListIndex = value;
                RaisePropertyChanged();
            }
        }

        private string GetLibraryListId(int index)
        {
            var flip = new List<LibraryListInfoPack>(APIhelper.Llipd.Values);

            if (index >= 0 && index < flip.Count)
            {
                return flip[index].ID;
            }
            return null;
        }

        #endregion LibraryListIndex変更通知プロパティ

        #region LibraryListHeaderImage変更通知プロパティ

        private string _LibraryListHeaderImage;

        public string LibraryListHeaderImage
        {
            get { return _LibraryListHeaderImage; }
            set
            {
                _LibraryListHeaderImage = String.IsNullOrEmpty(value)
                    ? @"pack://application:,,,/Resources/appbar.cd.fix.png"
                    : value;
                RaisePropertyChanged();
            }
        }

        #endregion LibraryListHeaderImage変更通知プロパティ

        #region LibraryListHeaderTitle変更通知プロパティ

        private string _LibraryListHeaderTitle;

        public string LibraryListHeaderTitle
        {
            get { return _LibraryListHeaderTitle; }
            set
            {
                _LibraryListHeaderTitle = String.IsNullOrEmpty(value) ? "N/A" : value;
                RaisePropertyChanged();
            }
        }

        #endregion LibraryListHeaderTitle変更通知プロパティ

        #region LibraryListHeaderArtist変更通知プロパティ

        private string _LibraryListHeaderArtist;

        public string LibraryListHeaderArtist
        {
            get { return _LibraryListHeaderArtist; }
            set
            {
                _LibraryListHeaderArtist = String.IsNullOrEmpty(value) ? "N/A" : value;
                RaisePropertyChanged();
            }
        }

        #endregion LibraryListHeaderArtist変更通知プロパティ

        #region LibraryList変更通知プロパティ

        private List<LibraryList> _LibraryList = new List<LibraryList>();

        public List<LibraryList> LibraryList
        {
            get { return _LibraryList; }
            set
            {
                if (_LibraryList == value)
                    return;
                _LibraryList = value;
                RaisePropertyChanged();
            }
        }

        #endregion LibraryList変更通知プロパティ

        #region PlayListIndex変更通知プロパティ

        private int _PlayListIndex;

        public int PlayListIndex
        {
            get { return _PlayListIndex; }
            set
            {
                if (_PlayListIndex == value)
                    return;
                _PlayListIndex = value;
                RaisePropertyChanged();
            }
        }

        #endregion PlayListIndex変更通知プロパティ

        #region PlayList変更通知プロパティ

        private List<LibraryList> _PlayList = new List<LibraryList>();

        public List<LibraryList> PlayList
        {
            get { return _PlayList; }
            set
            {
                _PlayListIndex = -1;
                if (_PlayList == value)
                    return;
                _PlayList = value;
                RaisePropertyChanged();
            }
        }

        #endregion PlayList変更通知プロパティ

        #region PlayId変更通知プロパティ

        private string _PlayId;

        public string PlayId
        {
            get { return _PlayId; }
            set
            {
                if (_PlayId == value)
                    return;
                _PlayId = value;
                RaisePropertyChanged();
            }
        }

        #endregion PlayId変更通知プロパティ

        #region Title変更通知プロパティ

        private string _Title = "";

        public string Title
        {
            get { return _Title; }
            set
            {
                if (_Title == value)
                    return;
                _Title = value;
                RaisePropertyChanged();
            }
        }

        #endregion Title変更通知プロパティ

        #region Artist変更通知プロパティ

        private string _Artist = "";

        public string Artist
        {
            get { return _Artist; }
            set
            {
                if (_Artist == value)
                    return;
                _Artist = value;
                RaisePropertyChanged();
            }
        }

        #endregion Artist変更通知プロパティ

        #region AlbumTitle変更通知プロパティ

        private string _AlbumTitle = "";

        public string AlbumTitle
        {
            get { return _AlbumTitle; }
            set
            {
                if (_AlbumTitle == value)
                    return;
                _AlbumTitle = value;
                RaisePropertyChanged();
            }
        }

        #endregion AlbumTitle変更通知プロパティ

        #region AlbumETC変更通知プロパティ

        private string _AlbumETC;

        public string AlbumETC
        {
            get { return _AlbumETC; }
            set
            {
                if (_AlbumETC == value)
                    return;
                _AlbumETC = value;
                RaisePropertyChanged();
            }
        }

        #endregion AlbumETC変更通知プロパティ

        #region CoverArt変更通知プロパティ

        private string _CoverArt;

        public string CoverArt
        {
            get { return _CoverArt; }
            set
            {
                _CoverArt = value;
                RaisePropertyChanged();
            }
        }

        #endregion CoverArt変更通知プロパティ

        #region ProgressBarMaxValue変更通知プロパティ

        private ulong _ProgressBarMaxValue = 12*60*1000;

        public ulong ProgressBarMaxValue
        {
            get { return _ProgressBarMaxValue; }
            set
            {
                if (_ProgressBarMaxValue == value)
                    return;
                _ProgressBarMaxValue = value;
                RaisePropertyChanged();
            }
        }

        #endregion ProgressBarMaxValue変更通知プロパティ

        #region ProgressBarCurrentValue変更通知プロパティ

        private bool ProgressBarCurrentValueLock;
        private double _ProgressBarCurrentValue;

        public double ProgressBarCurrentValue
        {
            get { return _ProgressBarCurrentValue; }
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
            if (!ProgressBarCurrentValueLock)
            {
                ProgressBarCurrentValueLock = true;

                ProgressBarCurrentValue = value;
                if (value > ProgressBarMaxValue)
                {
                    _sw.Stop();
                    Stop();

                    if (CurrentRepeatState == RepeatState.OnceRepeat)
                    {
                        Play();
                    }
                    else if (PlayListIndex + 1 < PlayList.Count)
                    {

                        PlayListIndex++;
                        PlayId = PlayList[PlayListIndex].ID;
                        Play();
                    }
                    else
                    {
                        if (CurrentRepeatState == RepeatState.Repeat)
                        {
                            PlayListIndex = 0;
                            PlayId = PlayList[PlayListIndex].ID;
                            Play();
                        }
                    }
                }

                ProgressBarCurrentValueLock = false;
            }
        }

        #endregion ProgressBarCurrentValue変更通知プロパティ

        #region ProgressBarIsIndeterminate変更通知プロパティ
        private bool _ProgressBarIsIndeterminate;

        public bool ProgressBarIsIndeterminate
        {
            get
            { return _ProgressBarIsIndeterminate; }
            set
            { 
                if (_ProgressBarIsIndeterminate == value)
                    return;
                _ProgressBarIsIndeterminate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Volume変更通知プロパティ

        private float _Volume = 0.5F;

        public float Volume
        {
            get { return _Volume; }
            set
            {
                if (_Volume == value)
                    return;
                _Volume = value;
                _model.ChangeVolume(AudioStep(value));
                VolumeString = Convert.ToInt32(value*100) + " %";
                RaisePropertyChanged();
            }
        }

        private static float AudioStep(float baseVolume)
        {
            return (float) (1 - Math.Cos(baseVolume*Math.PI/2));
        }

        #endregion Volume変更通知プロパティ

        #region VolumeString変更通知プロパティ

        private string _VolumeString;

        public string VolumeString
        {
            get { return _VolumeString; }
            set
            {
                if (_VolumeString == value)
                    return;
                _VolumeString = value;
                RaisePropertyChanged();
            }
        }

        #endregion VolumeString変更通知プロパティ

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

        #endregion ConfigWindowOpenCommand

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
            _model.Tweet(Title, Artist, AlbumTitle);
        }

        #endregion TweetCommand

        #region AddPlayListCommand

        private ViewModelCommand _AddPlayListCommand;

        public ViewModelCommand AddPlayListCommand
        {
            get
            {
                if (_AddPlayListCommand == null)
                {
                    _AddPlayListCommand = new ViewModelCommand(AddPlayList);
                }
                return _AddPlayListCommand;
            }
        }

        public void AddPlayList()
        {
            if (LibraryList.Count == 0) return;

            if (PlayList == null)
            {
                PlayList = new List<LibraryList>();
            }

            bool zeroFlag = PlayList.Count == 0;

            var tmpList = new List<LibraryList>(PlayList);
            int tmpIndex = PlayListIndex;

            foreach (LibraryList list in LibraryList.Where(list => list.IsDir == false))
            {
                tmpList.Add(new LibraryList
                {
                    ID = list.ID,
                    Artist = list.Artist,
                    Title = list.Title,
                    Track = String.Format("{0, 5}", (tmpList.Count + 1) + ". ")
                });
            }
            PlayListIndex = -1;
            PlayList = new List<LibraryList>(tmpList);
            PlayListIndex = tmpIndex;

            if (zeroFlag)
            {
                PlayId = PlayList[0].ID;
            }
        }

        #endregion AddPlayListCommand

        #region PlayPauseIcon変更通知プロパティ

        private Canvas _PlayPauseIcon = (Canvas) Application.Current.Resources["appbar_control_play"];

        public Canvas PlayPauseIcon
        {
            get { return _PlayPauseIcon; }
            set
            {
                if (Equals(_PlayPauseIcon, value))
                    return;
                _PlayPauseIcon = value;
                RaisePropertyChanged();
            }
        }

        #endregion PlayPauseIcon変更通知プロパティ

        #region PlayPauseIconMargin変更通知プロパティ

        private string _PlayPauseIconMargin = "4,0,0,0";

        public string PlayPauseIconMargin
        {
            get { return _PlayPauseIconMargin; }
            set
            {
                if (_PlayPauseIconMargin == value)
                    return;
                _PlayPauseIconMargin = value;
                RaisePropertyChanged();
            }
        }

        #endregion PlayPauseIconMargin変更通知プロパティ

        #region PlayPauseIconSize変更通知プロパティ

        private int _PlayPauseIconSize = 24;

        public int PlayPauseIconSize
        {
            get { return _PlayPauseIconSize; }
            set
            {
                if (_PlayPauseIconSize == value)
                    return;
                _PlayPauseIconSize = value;
                RaisePropertyChanged();
            }
        }

        #endregion PlayPauseIconSize変更通知プロパティ

        #region RepeatIcon変更通知プロパティ
        private Canvas _RepeatIcon = (Canvas)Application.Current.Resources["appbar_arrow_right"];

        public Canvas RepeatIcon
        {
            get
            { return _RepeatIcon; }
            set
            { 
                if (value == null)
                {
                    RepeatIconIsVisible = Visibility.Visible;
                }
                else
                {
                    RepeatIconIsVisible = Visibility.Hidden;
                }
                _RepeatIcon = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region RepeatIconIsVisible変更通知プロパティ
        private Visibility _RepeatIconIsVisible = Visibility.Hidden;

        public Visibility RepeatIconIsVisible
        {
            get
            { return _RepeatIconIsVisible; }
            set
            { 
                if (_RepeatIconIsVisible == value)
                    return;
                _RepeatIconIsVisible = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region CurrentRepeatState変更通知プロパティ
        private RepeatState _CurrentRepeatState = RepeatState.None;

        public RepeatState CurrentRepeatState
        {
            get
            { return _CurrentRepeatState; }
            set
            { 
                if (_CurrentRepeatState == value)
                    return;
                _CurrentRepeatState = value;
                RaisePropertyChanged();

                switch (value)
                {
                    case RepeatState.None:
                        RepeatIcon = (Canvas)Application.Current.Resources["appbar_arrow_right"];
                        break;


                    case RepeatState.Repeat:
                        RepeatIcon = (Canvas)Application.Current.Resources["appbar_repeat"];
                        break;


                    case RepeatState.OnceRepeat:
                        RepeatIcon = null;
                        break;
                }
            }
        }
        #endregion

        #region PlayState変更通知プロパティ
        private State _PlayState;

        public State PlayState
        {
            get
            { return _PlayState; }
            set
            { 
                if (_PlayState == value)
                    return;
                _PlayState = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PlayListFlyoutsIsOpen変更通知プロパティ
        private bool _PlayListFlyoutsIsOpen = false;

        public bool PlayListFlyoutsIsOpen
        {
            get
            { return _PlayListFlyoutsIsOpen; }
            set
            { 
                if (_PlayListFlyoutsIsOpen == value)
                    return;
                _PlayListFlyoutsIsOpen = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PlayListFlyoutsOpenCommand
        private ViewModelCommand _PlayListFlyoutsOpenCommand;

        public ViewModelCommand PlayListFlyoutsOpenCommand
        {
            get
            {
                if (_PlayListFlyoutsOpenCommand == null)
                {
                    _PlayListFlyoutsOpenCommand = new ViewModelCommand(PlayListFlyoutsOpen);
                }
                return _PlayListFlyoutsOpenCommand;
            }
        }

        public void PlayListFlyoutsOpen()
        {
            PlayListFlyoutsIsOpen = !PlayListFlyoutsIsOpen;
        }
        #endregion

        #region SearchCommand

        private ViewModelCommand _SearchCommand;

        public ViewModelCommand SearchCommand
        {
            get
            {
                if (_SearchCommand == null)
                {
                    _SearchCommand = new ViewModelCommand(Search);
                }
                return _SearchCommand;
            }
        }

        private string SearchLock { get; set; }

        public async void Search()
        {
            string guid = Guid.NewGuid().ToString();
            SearchLock = guid;

            await Task.Run(() =>
            {
                Thread.Sleep(500);

                if (SearchLock == guid)
                {
                    _model.Search(SearchQuery);
                }
            });
        }

        #endregion SearchCommand

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
            if (PlayState == State.Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Play()
        {
            if (PlayId == null)
            {
                return;
            }

            if (PlayState != State.Paused)
            {
                ProgressBarIsIndeterminate = true;
                _model.GetSongInfo(PlayId);
            }

            _model.Play(PlayId, AudioStep(Volume));
            SetPauseIcon();

            PlayState = State.Playing;
        }

        public void Pause()
        {
            _sw.Stop();
            _model.Pause();
            SetPlayIcon();
            PlayState = State.Paused;
        }

        public void SetPlayIcon()
        {
            PlayPauseIcon = (Canvas) Application.Current.Resources["appbar_control_play"];
            PlayPauseIconMargin = "4,0,0,0";
            PlayPauseIconSize = 24;
        }

        public void SetPauseIcon()
        {
            PlayPauseIcon = (Canvas) Application.Current.Resources["appbar_control_pause"];
            PlayPauseIconMargin = "0,0,0,0";
            PlayPauseIconSize = 20;
        }

        public void SetSongInfo()
        {
            StreamInfoPack sip = APIhelper.Sip;
            if (sip != null && sip.Status != "ok")
            {
                return;
            }

            if (sip != null)
            {
                ProgressBarMaxValue = Convert.ToUInt64(sip.Duration)*1000;

                Title = sip.Title;
                Artist = sip.Artist;
                AlbumTitle = sip.Album;
                AlbumETC = sip.Artist + " / " + sip.Album;
                CoverArt = sip.CoverArt;
            }
        }

        public void SetMusicFolder()
        {
            MusicFolderList = new List<string>();

            var flip = new List<FolderListInfoPack>(APIhelper.Flipd.Values);

            foreach (FolderListInfoPack folderList in flip)
            {
                MusicFolderList.Add(folderList.Name);
            }
        }

        public void SetLibraryList()
        {
            LibraryList = new List<LibraryList>();

            var llipd = new List<LibraryListInfoPack>(APIhelper.Llipd.Values);

            foreach (LibraryListInfoPack libraryList in llipd)
            {
                if (!String.IsNullOrEmpty(libraryList.Track))
                {
                    LibraryList.Add(new LibraryList
                    {
                        ID = libraryList.ID,
                        Album = libraryList.AlbumId,
                        Track = String.Format("{0, 5}", libraryList.Track + ". "),
                        Title = libraryList.Title,
                        Artist = " - " + libraryList.Artist,
                        IsDir = libraryList.IsDir
                    });
                }
                else
                {
                    LibraryList.Add(new LibraryList {Title = libraryList.Title, IsDir = libraryList.IsDir});
                }
            }

            if (llipd.Count != 0 && llipd[llipd.Count - 1].AlbumId != null)
            {
                _model.GetLibraryListHeader(llipd[llipd.Count - 1].AlbumId);
            }
        }

        public void SetLibraryListHeader()
        {
            var llipd = new List<LibraryListInfoPack>(APIhelper.Llipd.Values);

            if (LibraryListIndex != -1 && LibraryListIndex < llipd.Count)
            {
                LibraryListHeaderTitle = llipd[LibraryListIndex].Title;
            }
            LibraryListHeaderImage = null;
            LibraryListHeaderArtist = null;

            llipd = new List<LibraryListInfoPack>(APIhelper.Header.Values);

            if (llipd.Count != 0)
            {
                LibraryListHeaderImage = llipd[0].CoverArtUrl;

                if (llipd[0].Title != null)
                {
                    LibraryListHeaderTitle = llipd[0].Title;
                    LibraryListHeaderArtist = llipd[0].Artist;
                }
            }

            LibraryListIndex = -1;
        }

        #endregion PlayPauseCommand

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
            _model.Stop();
            _sw.Reset();
            PlayState = State.Stopped;
            PlayPauseIcon = (Canvas) Application.Current.Resources["appbar_control_play"];
            PlayPauseIconMargin = "4,0,0,0";
            PlayPauseIconSize = 24;
        }

        #endregion StopCommand

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
            if (PlayListIndex == 0)
            {
                Stop();
                Play();
            }
            else if (PlayListIndex - 1 >= 0)
            {
                if (_sw.ElapsedMilliseconds > 10 * 1000)
                {
                    Stop();
                    Play();
                }
                else
                {
                    PlayListIndex--;
                    PlayId = PlayList[PlayListIndex].ID;
                    if (PlayState != State.Stopped) Stop();
                    Play();
                }
            }
        }

        #endregion RewindCommand

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
            if (PlayListIndex + 1 < PlayList.Count)
            {
                PlayListIndex++;
                PlayId = PlayList[PlayListIndex].ID;
                if (PlayState != State.Stopped) Stop();
                Play();
            }
            else
            {
                PlayListIndex = 0;
                PlayId = PlayList[PlayListIndex].ID;
                if (PlayState != State.Stopped) Stop();
                Play();
            }
        }

        #endregion FastForwardCommand

        #region RepeatModeCommand
        private ViewModelCommand _RepeatModeCommand;

        public ViewModelCommand RepeatModeCommand
        {
            get
            {
                if (_RepeatModeCommand == null)
                {
                    _RepeatModeCommand = new ViewModelCommand(RepeatMode);
                }
                return _RepeatModeCommand;
            }
        }

        public void RepeatMode()
        {
            switch (CurrentRepeatState)
            {
                case RepeatState.None:
                    CurrentRepeatState = RepeatState.Repeat;
                    break;

                case RepeatState.Repeat:
                    CurrentRepeatState = RepeatState.OnceRepeat;
                    break;

                case RepeatState.OnceRepeat:
                    CurrentRepeatState = RepeatState.None;
                    break;
            }
        }
        #endregion

        public enum State
        {
            Playing,
            Paused,
            Stopped
        }

        public enum RepeatState
        {
            None,
            Repeat,
            OnceRepeat
        }
    }
}