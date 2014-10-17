using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Mánagarmr.Models;

namespace Mánagarmr.Helpers
{
    public class UpdateCheckHelper
    {
        /// <summary>
        /// バージョン情報を扱うクラス
        /// </summary>
        public class UpdateInfoPack
        {
            /// <summary>
            /// アップデート可能かどうか
            /// </summary>
            public bool UpdateAvailable { get; set; }

            /// <summary>
            /// 現在のバージョン
            /// </summary>
            public string CurrentVersion { get; set; }

            /// <summary>
            /// 配布中のバージョン
            /// </summary>
            public string AvailableVersion { get; set; }

            /// <summary>
            /// 配布URL
            /// </summary>
            public string DownloadURL { get; set; }
        }

        private string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        /// <summary>
        /// アップデートを確認します
        /// </summary>
        public async void UpdateCheck()
        {
            if (Settings.AllowUpdateCheck == false)
            {
                return;
            }

            UpdateInfoPack uip = await Task.Run(() => GetUpdateInfo(version));

            if (uip.UpdateAvailable == true)
            {
                if (File.Exists(Path.Combine(appPath, "SoftwareUpdater.exe")) == true)
                {
                    if (Settings.AllowAutoUpdate == true)
                    {
                        Process.Start(Path.Combine(appPath, "SoftwareUpdater.exe"));

                        Environment.Exit(0);
                    }
                }

                MessageBoxResult result = MessageBox.Show(
                    "新しいバージョンの Mánagarmr が見つかりました。\n" + uip.CurrentVersion + " -> " + uip.AvailableVersion + "\n\nダウンロードしますか？",
                    "アップデートのお知らせ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(uip.DownloadURL);
                }
            }
        }

        /// <summary>
        /// アップデートの確認をして、結果を UpdateInfoPack で返す
        /// </summary>
        /// <param name="currentVersion">現在のバージョン</param>
        /// <returns>アップデート情報のパック</returns>
        public UpdateInfoPack GetUpdateInfo(string currentVersion)
        {
            var currentVersionArray = VersionSplitter(currentVersion);
            var _uip = GetAvailableVersion();
            var availableVersionArray = VersionSplitter(_uip.AvailableVersion);

            var updateAvailable = false;

            for (int i = 0; i < 3; i++)
            {
                if (currentVersionArray[i] < availableVersionArray[i])
                {
                    updateAvailable = true;
                }
            }

            var uip = new UpdateInfoPack();
            uip.UpdateAvailable = updateAvailable;
            uip.CurrentVersion = string.Join(".", currentVersionArray);
            uip.AvailableVersion = _uip.AvailableVersion;
            uip.DownloadURL = _uip.DownloadURL; ;

            return uip;
        }

        /// <summary>
        /// 配布中のバージョンを取得する
        /// </summary>
        /// <returns>バージョン</returns>
        private UpdateInfoPack GetAvailableVersion()
        {
            var uip = new UpdateInfoPack();

            var hwreq = (HttpWebRequest)WebRequest.Create("http://api.ghippos.net/softwareupdate/managarmr/");

            try
            {
                using (var hwres = (HttpWebResponse)hwreq.GetResponse())
                using (var s = hwres.GetResponseStream())
                using (var xtr = new XmlTextReader(s))
                {
                    while (xtr.Read())
                    {
                        if (xtr.Name == "version")
                        {
                            uip.AvailableVersion = xtr.ReadString();
                        }
                        if (xtr.Name == "url")
                        {
                            uip.DownloadURL = xtr.ReadString();
                        }
                    }
                }
            }
            catch { }

            return uip;
        }

        /// <summary>
        /// X.X.X.X のバージョン表記を配列にする
        /// </summary>
        /// <param name="version">変換元のバージョン</param>
        /// <returns>変換後の配列</returns>
        private int[] VersionSplitter(string version)
        {
            if (version == "")
            {
                return new int[4] { 0, 0, 0, 0 };
            }

            var result = new int[3];
            var _result = new string[4];

            try
            {
                _result = version.Split('.');

                for (int i = 0; i < 3; i++)
                {
                    result[i] = int.Parse(_result[i]);
                }
            }
            catch
            {
                return new int[4] { 0, 0, 0, 0 };
            }

            return result;
        }
    }
}