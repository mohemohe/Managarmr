using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;
using Mánagarmr.Helpers;
using Mánagarmr.Models;

namespace Mánagarmr.ViewModels
{
    public class ToastViewModel : ViewModel
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

        private Stopwatch _sw;

        public async void Initialize()
        {
            Title = ToastHelper.Title;
            Artist = ToastHelper.Artist;
            Album = ToastHelper.Album;
            AlbumCoverUrl = ToastHelper.AlbumCoverImageUrl;

            FadeIn();

            _sw = new Stopwatch();
            _sw.Start();
            await Task.Run(() => Wait());
        }

        private void Wait()
        {
            while (true)
            {
                if (_sw.ElapsedMilliseconds > 5000)
                {
                    FadeOut();

                    while (true)
                    {
                        if (Opacity == 0.0)
                        {
                            Close();
                        }
                        else
                        {
                            Thread.Sleep(1);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private async void FadeIn()
        {
            for (double i = 0; i < 100; i++)
            {
                Opacity = i/100;
                await Task.Run(() => Thread.Sleep(3));
            }
        }

        private async void FadeOut()
        {
            for (double i = 100; i > 0; i--)
            {
                Opacity = i/100;
                await Task.Run(() => Thread.Sleep(3));
            }
        }

        #region Opacity変更通知プロパティ
        private double _Opacity = 0;

        public double Opacity
        {
            get
            { return _Opacity; }
            set
            { 
                if (_Opacity == value)
                    return;
                _Opacity = value;
                RaisePropertyChanged();
            }
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

        #region Album変更通知プロパティ
        private string _Album;

        public string Album
        {
            get
            { return _Album; }
            set
            { 
                if (_Album == value)
                    return;
                _Album = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AlbumCoverUrl変更通知プロパティ
        private string _AlbumCoverUrl;

        public string AlbumCoverUrl
        {
            get
            { return _AlbumCoverUrl; }
            set
            { 
                if (_AlbumCoverUrl == value)
                    return;
                _AlbumCoverUrl = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region CloseCommand
        private ViewModelCommand _CloseCommand;

        public ViewModelCommand CloseCommand
        {
            get
            {
                if (_CloseCommand == null)
                {
                    _CloseCommand = new ViewModelCommand(Close);
                }
                return _CloseCommand;
            }
        }

        public void Close()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
        }
        #endregion

        #region TimerResetCommand
        private ViewModelCommand _TimerResetCommand;

        public ViewModelCommand TimerResetCommand
        {
            get
            {
                if (_TimerResetCommand == null)
                {
                    _TimerResetCommand = new ViewModelCommand(TimerReset);
                }
                return _TimerResetCommand;
            }
        }

        public void TimerReset()
        {
            _sw.Restart();
        }
        #endregion

    }
}
