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
using System.Reflection;
using Mánagarmr.Models.SubsonicAPI;
using System.Threading.Tasks;
using System.Diagnostics;
using NAudio.Wave;

namespace Mánagarmr.ViewModels
{
    public class ConfigWindowViewModel : ViewModel
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


        #region SelectedIndex変更通知プロパティ
        private string _SelectedIndex;

        public string SelectedIndex
        {
            get
            { return _SelectedIndex; }
            set
            {
                if (_SelectedIndex == value)
                    return;
                _SelectedIndex = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AllowUpdateCheck変更通知プロパティ
        private bool _AllowUpdateCheck;

        public bool AllowUpdateCheck
        {
            get
            { return _AllowUpdateCheck; }
            set
            {
                if (_AllowUpdateCheck == value)
                    return;
                _AllowUpdateCheck = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AllowAutoUpdate変更通知プロパティ
        private bool _AllowAutoUpdate;

        public bool AllowAutoUpdate
        {
            get
            { return _AllowAutoUpdate; }
            set
            {
                if (_AllowAutoUpdate == value)
                    return;
                _AllowAutoUpdate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PrimaryServerAddress変更通知プロパティ
        private string _PrimaryServerAddress;

        public string PrimaryServerAddress
        {
            get
            { return _PrimaryServerAddress; }
            set
            { 
                if (_PrimaryServerAddress == value)
                    return;
                _PrimaryServerAddress = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SecondaryServerAddress変更通知プロパティ
        private string _SecondaryServerAddress;

        public string SecondaryServerAddress
        {
            get
            { return _SecondaryServerAddress; }
            set
            { 
                if (_SecondaryServerAddress == value)
                    return;
                _SecondaryServerAddress = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IgnoreSSLcertificateError変更通知プロパティ
        private bool _IgnoreSSLcertificateError;

        public bool IgnoreSSLcertificateError
        {
            get
            { return _IgnoreSSLcertificateError; }
            set
            {
                if (_IgnoreSSLcertificateError == value)
                    return;
                _IgnoreSSLcertificateError = value;
                IgnoreSSLcertificateErrorIsEnabled = !value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IgnoreSSLcertificateErrorIsEnabled変更通知プロパティ
        private bool _IgnoreSSLcertificateErrorIsEnabled;

        public bool IgnoreSSLcertificateErrorIsEnabled
        {
            get
            { return _IgnoreSSLcertificateErrorIsEnabled; }
            set
            { 
                if (_IgnoreSSLcertificateErrorIsEnabled == value)
                    return;
                _IgnoreSSLcertificateErrorIsEnabled = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SubsonicID変更通知プロパティ
        private string _SubsonicID;

        public string SubsonicID
        {
            get
            { return _SubsonicID; }
            set
            { 
                if (_SubsonicID == value)
                    return;
                _SubsonicID = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SubsonicPassword変更通知プロパティ
        private string _SubsonicPassword;

        public string SubsonicPassword
        {
            get
            { return _SubsonicPassword; }
            set
            { 
                if (_SubsonicPassword == value)
                    return;
                _SubsonicPassword = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region SubsonicConnectionTest変更通知プロパティ
        private string _SubsonicConnectionTest;

        public string SubsonicConnectionTest
        {
            get
            { return _SubsonicConnectionTest; }
            set
            { 
                if (_SubsonicConnectionTest == value)
                    return;
                _SubsonicConnectionTest = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region NetworkBufferSlider変更通知プロパティ
        private int _NetworkBufferSliderValue = 10;

        public int NetworkBufferSliderValue
        {
            get
            { return _NetworkBufferSliderValue; }
            set
            {
                if (_NetworkBufferSliderValue == value)
                    return;
                _NetworkBufferSliderValue = value;
                RaisePropertyChanged();

                NetworkBufferSliderValueString = value + "sec";
            }
        }
        #endregion

        #region NetworkBufferSliderValueString変更通知プロパティ
        private string _NetworkBufferSliderValueString;

        public string NetworkBufferSliderValueString
        {
            get
            { return _NetworkBufferSliderValueString; }
            set
            { 
                if (_NetworkBufferSliderValueString == value)
                    return;
                _NetworkBufferSliderValueString = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AudioMethodId変更通知プロパティ
        private int _AudioMethodId = 0;

        public int AudioMethodId
        {
            get
            { return _AudioMethodId; }
            set
            { 
                if (_AudioMethodId == value)
                    return;
                _AudioMethodId = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AudioDeviceList変更通知プロパティ
        private List<string> _AudioDeviceList;

        public List<string> AudioDeviceList
        {
            get
            { return _AudioDeviceList; }
            set
            { 
                if (_AudioDeviceList == value)
                    return;
                _AudioDeviceList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AudioDeviceListIndex変更通知プロパティ
        private int _AudioDeviceListIndex;

        public int AudioDeviceListIndex
        {
            get
            { return _AudioDeviceListIndex; }
            set
            { 
                if (_AudioDeviceListIndex == value)
                    return;
                _AudioDeviceListIndex = value;
                RaisePropertyChanged();

                AudioDeviceName = AudioDeviceList[value];
            }
        }
        #endregion

        #region AudioDeviceName変更通知プロパティ
        private string _AudioDeviceName;

        public string AudioDeviceName
        {
            get
            { return _AudioDeviceName; }
            set
            { 
                if (_AudioDeviceName == value)
                    return;
                _AudioDeviceName = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TargetBitrate変更通知プロパティ
        private int _TargetBitrate;

        public int TargetBitrate
        {
            get
            { return _TargetBitrate; }
            set
            { 
                if (_TargetBitrate == value)
                    return;
                _TargetBitrate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TargetBitrateList変更通知プロパティ
        private List<int> _TargetBitrateList;

        public List<int> TargetBitrateList
        {
            get
            { return _TargetBitrateList; }
            set
            { 
                if (_TargetBitrateList == value)
                    return;
                _TargetBitrateList = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TargetBitrateSelectedIndex変更通知プロパティ
        private int _TargetBitrateSelectedIndex = 5;

        public int TargetBitrateSelectedIndex
        {
            get
            { return _TargetBitrateSelectedIndex; }
            set
            { 
                if (_TargetBitrateSelectedIndex == value)
                    return;
                _TargetBitrateSelectedIndex = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AudioBufferSliderValue変更通知プロパティ
        private int _AudioBufferSliderValue = 250;

        public int AudioBufferSliderValue
        {
            get
            { return _AudioBufferSliderValue; }
            set
            {
                if (_AudioBufferSliderValue == value)
                    return;
                _AudioBufferSliderValue = value;
                RaisePropertyChanged();

                AudioBufferSliderValueString = value + "ms";
            }
        }
        #endregion

        #region AudioBufferSliderValueString変更通知プロパティ
        private string _AudioBufferSliderValueString;

        public string AudioBufferSliderValueString
        {
            get
            { return _AudioBufferSliderValueString; }
            set
            {
                if (_AudioBufferSliderValueString == value)
                    return;
                _AudioBufferSliderValueString = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TweetTemplate変更通知プロパティ
        private string _TweetTemplate;

        public string TweetTemplate
        {
            get
            { return _TweetTemplate; }
            set
            { 
                if (_TweetTemplate == value)
                    return;
                _TweetTemplate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TweetUrl変更通知プロパティ
        private int _TweetUrl;

        public int TweetUrl
        {
            get
            { return _TweetUrl; }
            set
            { 
                if (_TweetUrl == value)
                    return;
                _TweetUrl = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TwitterAuthPIN変更通知プロパティ
        private string _TwitterAuthPIN;

        public string TwitterAuthPIN
        {
            get
            { return _TwitterAuthPIN; }
            set
            { 
                if (_TwitterAuthPIN == value)
                    return;
                _TwitterAuthPIN = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TwitterAccessToken変更通知プロパティ
        private string _TwitterAccessToken;

        public string TwitterAccessToken
        {
            get
            { return _TwitterAccessToken; }
            set
            { 
                if (_TwitterAccessToken == value)
                    return;
                _TwitterAccessToken = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TwitterAccessTokenSecret変更通知プロパティ
        private string _TwitterAccessTokenSecret;

        public string TwitterAccessTokenSecret
        {
            get
            { return _TwitterAccessTokenSecret; }
            set
            { 
                if (_TwitterAccessTokenSecret == value)
                    return;
                _TwitterAccessTokenSecret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TwitterIsAuthed変更通知プロパティ
        private bool _TwitterIsAuthed = false;

        public bool TwitterIsAuthed
        {
            get
            { return _TwitterIsAuthed; }
            set
            { 
                if (_TwitterIsAuthed == value)
                    return;
                _TwitterIsAuthed = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TwitterAuthProgress変更通知プロパティ
        private string _TwitterAuthProgress;

        public string TwitterAuthProgress
        {
            get
            { return _TwitterAuthProgress; }
            set
            { 
                if (_TwitterAuthProgress == value)
                    return;
                _TwitterAuthProgress = value;
                RaisePropertyChanged();
            }
        }

        private string[] TwitterAuthProgressMessage = { "認証されていません。 ①のボタンをクリックして認証してください。",
                                                        "Mánagarmr へのアクセスを許可して、PINを入力してください。",
                                                        "認証されています。"};
        #endregion

        #region Language変更通知プロパティ
        //private ObservableCollection<LanguageViewModel> _Language;

        //public ObservableCollection<LanguageViewModel> Language
        //{
        //    get
        //    { return _Language; }
        //    set
        //    {
        //        if (_Language == value)
        //            return;
        //        _Language = value;
        //        RaisePropertyChanged();
        //    }
        //}
        #endregion

        #region Version変更通知プロパティ
        private string _Version;

        public string Version
        {
            get
            { return _Version; }
            set
            {
                if (_Version == value)
                    return;
                _Version = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        Twitter twitter = new Twitter();
        WaveOut wo = new WaveOut();

        public void Initialize()
        {
            Version = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //this.Language = new ObservableCollection<LanguageViewModel>() {
            //    new LanguageViewModel() { Language = "日本語 (ja-JP)", Locale = null },
            //    new LanguageViewModel() { Language = "English (en-US)", Locale = "en-US" },
            //    new LanguageViewModel() { Language = "こふ語 (kv-JP)", Locale = "ja" },
            //};

            TargetBitrateList = new List<int>() { 32, 48, 64, 80, 96, 128, 160, 192, 224, 256, 320 };
            AudioDeviceList = new List<string> {"Default"};

            ReadSettings();

            if (String.IsNullOrEmpty(TwitterAccessToken) == false && String.IsNullOrEmpty(TwitterAccessTokenSecret) == false)
            {
                TwitterIsAuthed = true;
                TwitterAuthProgress = TwitterAuthProgressMessage[2];
            }
            else
            {
                TwitterAuthProgress = TwitterAuthProgressMessage[0];
            }

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var gc = WaveOut.GetCapabilities(i);
                AudioDeviceList.Add(gc.ProductName);
                if (gc.ProductName == Settings.AudioDevice)
                {
                    // 0はDefault
                    AudioDeviceListIndex = i + 1;
                }
                else
                {
                    AudioDeviceListIndex = 0;
                }
            }

            TargetBitrateSelectedIndex = TargetBitrateList.IndexOf(TargetBitrate);

            NetworkBufferSliderValueString = NetworkBufferSliderValue + "sec";
            AudioBufferSliderValueString = AudioBufferSliderValue + "ms";
        }

        private void ReadSettings()
        {
            AllowUpdateCheck = Settings.AllowUpdateCheck;
            AllowAutoUpdate = Settings.AllowAutoUpdate;

            PrimaryServerAddress = Settings.PrimaryServerUrl;
            SecondaryServerAddress = Settings.SecondaryServerUrl;
            IgnoreSSLcertificateError = Settings.IgnoreSSLcertificateError;
            IgnoreSSLcertificateErrorIsEnabled = !Settings.IgnoreSSLcertificateError;
            SubsonicID = Settings.UserName;
            SubsonicPassword = Settings.Password;

            TargetBitrate = Settings.TargetBitrate;
            NetworkBufferSliderValue = Settings.NetworkBuffer;

            TwitterAccessToken = Settings.AccessToken;
            TwitterAccessTokenSecret = Settings.AccessTokenSecret;
            TweetTemplate = Settings.TweetTextFormat;
            TweetUrl = Settings.TweetUrl;

            AudioMethodId = Settings.AudioMethod;
            AudioDeviceName = Settings.AudioDevice;
            AudioBufferSliderValue = Settings.AudioBuffer;

            //Locale = Settings.Language;
        }

        private void SaveSettings()
        {
            Settings.AllowUpdateCheck = AllowUpdateCheck;
            Settings.AllowAutoUpdate = AllowAutoUpdate;

            Settings.PrimaryServerUrl = PrimaryServerAddress;
            Settings.SecondaryServerUrl = SecondaryServerAddress;
            Settings.IgnoreSSLcertificateError = IgnoreSSLcertificateError;
            Settings.UserName = SubsonicID;
            Settings.Password = SubsonicPassword;

            Settings.TargetBitrate = TargetBitrate;
            Settings.NetworkBuffer = NetworkBufferSliderValue;

            Settings.AccessToken = TwitterAccessToken;
            Settings.AccessTokenSecret = TwitterAccessTokenSecret;
            Settings.TweetTextFormat = TweetTemplate;
            Settings.TweetUrl = TweetUrl;

            Settings.AudioMethod = AudioMethodId;
            Settings.AudioDevice = AudioDeviceName;
            Settings.AudioBuffer = AudioBufferSliderValue;

            //Settings.Language = Locale;
        }

        #region ConnectionTestCommand
        private ViewModelCommand _ConnectionTestCommand;

        public ViewModelCommand ConnectionTestCommand
        {
            get
            {
                if (_ConnectionTestCommand == null)
                {
                    _ConnectionTestCommand = new ViewModelCommand(ConnectionTest);
                }
                return _ConnectionTestCommand;
            }
        }

        public async void ConnectionTest()
        {
            var p = new Ping();
            string PrimaryServerStatus = "checking...";
            string SecondaryServerStatus = "waiting...";

            SubsonicConnectionTest = "Primary server: " + PrimaryServerStatus + "\n" +
                                     "Secondary server: " + SecondaryServerStatus;

            // Primary server
            var url = PrimaryServerAddress;
            if (url.EndsWith("/") == false)
            {
                url = url + "/";
            }

            var result = false;
            await Task.Run(() => p.CheckServer(url, IgnoreSSLcertificateError, SubsonicID, SubsonicPassword, out result));
            if (result == true)
            {
                PrimaryServerStatus = "OK!";
            }
            else
            {
                PrimaryServerStatus = "Cant reach server :(";
            }
            SecondaryServerStatus = "checking...";
            SubsonicConnectionTest = "Primary server: " + PrimaryServerStatus + "\n" +
                                     "Secondary server: " + SecondaryServerStatus;

            // Secondary server
            url = SecondaryServerAddress;
            if (url.EndsWith("/") == false)
            {
                url = url + "/";
            }

            result = false;
            await Task.Run(() => p.CheckServer(url, IgnoreSSLcertificateError, SubsonicID, SubsonicPassword, out result));
            if (result == true)
            {
                SecondaryServerStatus = "OK!";
            }
            else
            {
                SecondaryServerStatus = "Cant reach server :(";
            }

            SubsonicConnectionTest = "Primary server: " + PrimaryServerStatus + "\n" +
                                     "Secondary server: " + SecondaryServerStatus;
        }
        #endregion

        #region OpenAuthUrlCommand
        private ViewModelCommand _OpenAuthUrlCommand;

        public ViewModelCommand OpenAuthUrlCommand
        {
            get
            {
                if (_OpenAuthUrlCommand == null)
                {
                    _OpenAuthUrlCommand = new ViewModelCommand(OpenAuthUrl);
                }
                return _OpenAuthUrlCommand;
            }
        }

        public void OpenAuthUrl()
        {
            var url = twitter.GetOAuthUrl();
            if (String.IsNullOrEmpty(url) == false)
            {
                Process.Start(url);
                TwitterAuthProgress = TwitterAuthProgressMessage[1];
            }
        }
        #endregion

        #region GetAccessTokenCommand
        private ViewModelCommand _GetAccessTokenCommand;

        public ViewModelCommand GetAccessTokenCommand
        {
            get
            {
                if (_GetAccessTokenCommand == null)
                {
                    _GetAccessTokenCommand = new ViewModelCommand(GetAccessToken);
                }
                return _GetAccessTokenCommand;
            }
        }

        public void GetAccessToken()
        {
            if (String.IsNullOrEmpty(TwitterAuthPIN) == false)
            {
                string twitterAccessToken;
                string twitterAccessTokenSecret;
                twitter.GetAccessToken(TwitterAuthPIN, out twitterAccessToken, out twitterAccessTokenSecret);
                TwitterAccessToken = twitterAccessToken;
                TwitterAccessTokenSecret = twitterAccessTokenSecret;

                TwitterAuthProgress = TwitterAuthProgressMessage[2];
            }
        }
        #endregion

        #region OKCommand
        private ViewModelCommand _OKCommand;

        public ViewModelCommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new ViewModelCommand(OK);
                }
                return _OKCommand;
            }
        }

        public void OK()
        {
            SaveSettings();
            Close();
        }
        #endregion

        #region CancelCommand
        private ViewModelCommand _CancelCommand;

        public ViewModelCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new ViewModelCommand(Cancel);
                }
                return _CancelCommand;
            }
        }

        public void Cancel()
        {
            Close();
        }
        #endregion

        #region ApplyCommand
        private ViewModelCommand _ApplyCommand;

        public ViewModelCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new ViewModelCommand(Apply);
                }
                return _ApplyCommand;
            }
        }

        public void Apply()
        {
            SaveSettings();
        }
        #endregion

        private void Close()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
        }
    }
}
