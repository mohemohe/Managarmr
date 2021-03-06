﻿using System;
using System.IO;
using System.Windows;

using Livet;
using Mánagarmr.Helpers;
using Mánagarmr.Models;
using Mánagarmr.Models.SubsonicAPI;
using Mánagarmr.Views;
using System.Threading;
using System.Threading.Tasks;

namespace Mánagarmr
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var uch = new UpdateCheckHelper();
            uch.UpdateCheck();

            if (Settings.Initialize())
            {
                APIhelper.BuildBaseUrl();
            }

            var mw = new MainWindow();
            mw.Show();
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var messeage = "未知のエラーが発生しました。アプリケーションを終了します。\n\n";
            string extMesseage = null;

            try
            {
                var ex = (Exception) e.ExceptionObject;

                messeage = "未知のエラーが発生しました。アプリケーションを終了します。\n\nエラー内容:\n";

                try
                {
                    var location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    using (var sw = new StreamWriter(location + @"\log.txt"))
                    {
                        sw.WriteLine("Messeage:");
                        sw.WriteLine(ex.Message);
                        sw.WriteLine("");
                        sw.WriteLine("InnerException:");
                        sw.WriteLine(ex.InnerException);
                        sw.WriteLine("");
                        sw.WriteLine("Source:");
                        sw.WriteLine(ex.Source);
                        sw.WriteLine("");
                        sw.WriteLine("TargetSite:");
                        sw.WriteLine(ex.TargetSite);
                        sw.WriteLine("");
                        sw.WriteLine("");
                        sw.WriteLine("StackTrace:");
                        sw.WriteLine(ex.StackTrace);
                        sw.Close();
                    }

                    extMesseage = "\n\n\n実行フォルダに log.txt を生成しました。";
                }
                catch { }

                MessageBoxHelper.AddMessageBoxQueue(new MessageBoxPack(
                    messeage + ex.Message + extMesseage,
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
            }
            catch
            {
                MessageBoxHelper.AddMessageBoxQueue(new MessageBoxPack(
                    messeage + "不明なエラー",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
            }
            finally
            {
                Environment.Exit(1);
            }
        }
    }
}
