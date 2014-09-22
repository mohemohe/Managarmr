using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Mánagarmr.Models
{
    /// <summary>
    ///  XMLに書き出すための動的クラス
    /// </summary>
    public class XMLSettings
    {
        public byte[] Hash;

        public string PrimaryServerUrl;
        public string SecondaryServerUrl;
        public bool IgnoreSSLcertificateError;
        public string UserName;
        public string Password;

        public int AudioMethod;
        public int AudioDevice;
        public int AudioBuffer;
        public float Volume;

        public string TweetTextFormat = "NowPlaying: %title% - %artist% / %album%";
        public string AccessToken;
        public string AccessTokenSecret;

        public bool AllowUpdateCheck = true;
        public bool AllowAutoUpdate = true;

        public string Language;
    }

    /// <summary>
    ///  設定を読み書きするクラス
    /// </summary>
    internal class Settings
    {
        # region Memory
        /// <summary>
        ///  実際の設定値はここに記憶される
        /// </summary>
        protected class _Settings
        {
            public static byte[] _Hash { get; set; }

            public static string _PrimaryServerUrl { get; set; }
            public static string _SecondaryServerUrl { get; set; }
            public static bool _IgnoreSSLcertificateError { get; set; }
            public static string _UserName { get; set; }
            public static string _Password { get; set; }

            public static int _AudioMethod { get; set; }
            public static int _AudioDevice { get; set; }
            public static int _AudioBuffer { get; set; }
            public static float _Volume { get; set; }

            public static string _TweetTextFormat { get; set; }
            public static string _AccessToken { get; set; }
            public static string _AccessTokenSecret { get; set; }

            public static bool? _AllowUpdateCheck { get; set; }
            public static bool? _AllowAutoUpdate { get; set; }

            public static string _Language { get; set; }
        }
        #endregion

        #region Accessor

        /// <summary>
        ///  PCごとの簡易ハッシュ値
        /// </summary>
        public static byte[] Hash
        {
            get { return _Settings._Hash; }
            set { _Settings._Hash = value; }
        }

        public static string PrimaryServerUrl
        {
            get { return _Settings._PrimaryServerUrl; }
            set { _Settings._PrimaryServerUrl = value; }
        }

        public static string SecondaryServerUrl
        {
            get { return _Settings._SecondaryServerUrl; }
            set { _Settings._SecondaryServerUrl = value; }
        }
        public static bool IgnoreSSLcertificateError
        {
            get { return _Settings._IgnoreSSLcertificateError; }
            set { _Settings._IgnoreSSLcertificateError = value; }
        }
        public static string UserName
        {
            get
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._UserName, seed);
            }
            set
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._UserName = Crypt.Encrypt(value, seed);
            }
        }

        public static string Password
        {
            get
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._Password, seed);
            }
            set
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._Password = Crypt.Encrypt(value, seed);
            }
        }

        public static int AudioMethod
        {
            get { return _Settings._AudioMethod; }
            set { _Settings._AudioMethod = value; }
        }

        public static int AudioDevice
        {
            get { return _Settings._AudioDevice; }
            set { _Settings._AudioDevice = value; }
        }

        public static int AudioBuffer
        {
            get { return _Settings._AudioBuffer; }
            set { _Settings._AudioBuffer = value; }
        }

        public static float Volume
        {
            get { return _Settings._Volume; }
            set { _Settings._Volume = value; }
        }

        public static string TweetTextFormat
        {
            get { return _Settings._TweetTextFormat; }
            set { _Settings._TweetTextFormat = value; }
        }

        public static string AccessToken
        {
            get { return _Settings._AccessToken; }
            set { _Settings._AccessToken = value; }
        }

        public static string AccessTokenSecret
        {
            get
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._AccessTokenSecret, seed);
            }
            set
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._AccessTokenSecret = Crypt.Encrypt(value, seed);
            }
        }

        public static bool AllowUpdateCheck
        {
            get
            {
                if (_Settings._AllowUpdateCheck == null) { return true; }
                else { return (bool)_Settings._AllowUpdateCheck; }
            }
            set { _Settings._AllowAutoUpdate = value; }
        }

        public static bool AllowAutoUpdate
        {
            get
            {
                if (_Settings._AllowAutoUpdate == null) { return true; }
                else { return (bool)_Settings._AllowAutoUpdate; }
            }
            set { _Settings._AllowAutoUpdate = value; }
        }

        public static string Language
        {
            get { return _Settings._Language; }
            set { _Settings._Language = value; }
        }

        #endregion Accessor

        internal static string ConsumerKey { get { return "WY93gCVpkgAMdVAVuu5yDYB0g"; } }
        internal static string ConsumerSecret { get { return "Q1GYjZd9F78NdbpCStl9u0lbMcqJGmJjqXnZS9NQqddMyqwS5t"; } }

        private static string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string fileName = "Settings.xml";
        private static string filePath = Path.Combine(appPath, fileName);

        /// <summary>
        ///  設定を読み込むのに必要なシード値を生成し、設定の読み込みを試行する
        /// </summary>
        /// <returns>設定を読み込めたかどうか</returns>
        public static bool Initialize()
        {
            var mn = Environment.MachineName;
            var un = Environment.UserName;
            var udn = Environment.UserDomainName;

            var rawHash = Crypt.CreateSeed(mn + un + udn);
            var hash = Crypt.CreateSeed(rawHash);

            if (File.Exists(filePath) == true)
            {
                ReadSettings();

                if (Settings.Hash.SequenceEqual(hash) == false)
                {
                    Settings.UserName = "";
                    Settings.Password = "";

                    return false;
                }

                return true;
            }
            else
            {
                Settings.UserName = "";
                Settings.Password = "";
                Settings.Hash = hash;

                Settings.AudioBuffer = 250;

                return false;
            }
        }

        /// <summary>
        ///  ファイルから設定を読み込む
        /// </summary>
        private static void ReadSettings()
        {
            var xmls = new XMLSettings();
            var xs = new XmlSerializer(typeof(XMLSettings));
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                xmls = (XMLSettings)xs.Deserialize(fs);
                fs.Close();
            }

            _Settings._Hash = xmls.Hash;

            _Settings._PrimaryServerUrl = xmls.PrimaryServerUrl;
            _Settings._SecondaryServerUrl = xmls.SecondaryServerUrl;
            _Settings._IgnoreSSLcertificateError = xmls.IgnoreSSLcertificateError;
            _Settings._UserName = xmls.UserName;
            _Settings._Password = xmls.Password;

            _Settings._AudioMethod = xmls.AudioMethod;
            _Settings._AudioDevice = xmls.AudioDevice;
            _Settings._AudioBuffer = xmls.AudioBuffer;
            _Settings._Volume = xmls.Volume;

            if (xmls.TweetTextFormat != null)
            {
                _Settings._TweetTextFormat = xmls.TweetTextFormat;
            }
            _Settings._AccessToken = xmls.AccessToken;
            _Settings._AccessTokenSecret = xmls.AccessTokenSecret;

            _Settings._AllowUpdateCheck = xmls.AllowUpdateCheck;
            _Settings._AllowAutoUpdate = xmls.AllowAutoUpdate;

            _Settings._Language = xmls.Language;
        }

        /// <summary>
        ///  ファイルへ設定を書き込む
        /// </summary>
        public static void WriteSettings()
        {
            var xmls = new XMLSettings();

            xmls.Hash = _Settings._Hash;

            xmls.PrimaryServerUrl = _Settings._PrimaryServerUrl;
            xmls.SecondaryServerUrl = _Settings._SecondaryServerUrl;
            xmls.IgnoreSSLcertificateError = _Settings._IgnoreSSLcertificateError;
            xmls.UserName = _Settings._UserName;
            xmls.Password = _Settings._Password;

            xmls.AudioMethod = _Settings._AudioMethod;
            xmls.AudioDevice = _Settings._AudioDevice;
            xmls.AudioBuffer = _Settings._AudioBuffer;
            xmls.Volume = _Settings._Volume;

            xmls.TweetTextFormat = _Settings._TweetTextFormat;
            xmls.AccessToken = _Settings._AccessToken;
            xmls.AccessTokenSecret = _Settings._AccessTokenSecret;

            xmls.AllowUpdateCheck = Settings.AllowUpdateCheck;
            xmls.AllowAutoUpdate = Settings.AllowAutoUpdate;

            xmls.Language = Settings.Language;

            var xs = new XmlSerializer(typeof(XMLSettings));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, xmls);
            }
        }
    }
}
