using Livet;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.RightsManagement;
using System.Xml.Serialization;

namespace Mánagarmr.Models
{
    /// <summary>
    ///     XMLに書き出すための動的クラス
    /// </summary>
    public class XMLSettings
    {
        public string AccessToken;
        public string AccessTokenSecret;
        public bool? AllowAutoUpdate;
        public bool? AllowUpdateCheck;
        public int AudioBuffer;
        public string AudioDevice;
        public int AudioMethod;
        public byte[] Hash;

        public bool IgnoreSSLcertificateError;
        public string Language;

        public int NetworkBuffer;
        public string Password;
        public string PrimaryServerUrl;
        public string SecondaryServerUrl;
        public int TargetBitrate;

        public string TweetTextFormat;
        public int TweetUrl;
        public string UserName;
        public float Volume = 0.5F;
        public int RepeatState;
    }

    /// <summary>
    ///     設定を読み書きするクラス
    /// </summary>
    internal class Settings
    {
        # region Memory

        /// <summary>
        ///     実際の設定値はここに記憶される
        /// </summary>
        protected class _Settings
        {
            public static byte[] _Hash { get; set; }

            public static string _PrimaryServerUrl { get; set; }

            public static string _SecondaryServerUrl { get; set; }

            public static bool _IgnoreSSLcertificateError { get; set; }

            public static string _UserName { get; set; }

            public static string _Password { get; set; }

            public static int _NetworkBuffer { get; set; }

            public static int _TargetBitrate { get; set; }

            public static int _AudioMethod { get; set; }

            public static string _AudioDevice { get; set; }

            public static int _AudioBuffer { get; set; }

            public static float _Volume { get; set; }

            public static int _RepeatState { get; set; }

            public static string _TweetTextFormat { get; set; }

            public static int _TweetUrl { get; set; }

            public static string _AccessToken { get; set; }

            public static string _AccessTokenSecret { get; set; }

            public static bool? _AllowUpdateCheck { get; set; }

            public static bool? _AllowAutoUpdate { get; set; }

            public static string _Language { get; set; }
        }

        #endregion

        #region Accessor

        /// <summary>
        ///     PCごとの簡易ハッシュ値
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
                string mn = Environment.MachineName;
                string un = Environment.UserName;
                string udn = Environment.UserDomainName;

                byte[] seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._UserName, seed);
            }
            set
            {
                string mn = Environment.MachineName;
                string un = Environment.UserName;
                string udn = Environment.UserDomainName;

                byte[] seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._UserName = Crypt.Encrypt(value, seed);
            }
        }

        public static string Password
        {
            get
            {
                string mn = Environment.MachineName;
                string un = Environment.UserName;
                string udn = Environment.UserDomainName;

                byte[] seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._Password, seed);
            }
            set
            {
                string mn = Environment.MachineName;
                string un = Environment.UserName;
                string udn = Environment.UserDomainName;

                byte[] seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._Password = Crypt.Encrypt(value, seed);
            }
        }

        public static int NetworkBuffer
        {
            get { return _Settings._NetworkBuffer; }
            set { _Settings._NetworkBuffer = value; }
        }

        public static int TargetBitrate
        {
            get { return _Settings._TargetBitrate; }
            set { _Settings._TargetBitrate = value; }
        }

        public static int AudioMethod
        {
            get { return _Settings._AudioMethod; }
            set { _Settings._AudioMethod = value; }
        }

        public static string AudioDevice
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

        public static int RepeatState
        {
            get { return _Settings._RepeatState; }
            set { _Settings._RepeatState = value; }
        }

        public static string TweetTextFormat
        {
            get { return _Settings._TweetTextFormat; }
            set { _Settings._TweetTextFormat = value; }
        }

        public static int TweetUrl
        {
            get { return _Settings._TweetUrl; }
            set { _Settings._TweetUrl = value; }
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
                string mn = Environment.MachineName;
                string un = Environment.UserName;
                string udn = Environment.UserDomainName;

                byte[] seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._AccessTokenSecret, seed);
            }
            set
            {
                string mn = Environment.MachineName;
                string un = Environment.UserName;
                string udn = Environment.UserDomainName;

                byte[] seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._AccessTokenSecret = Crypt.Encrypt(value, seed);
            }
        }

        public static bool AllowUpdateCheck
        {
            get
            {
                if (_Settings._AllowUpdateCheck == null)
                {
                    return true;
                }
                return (bool) _Settings._AllowUpdateCheck;
            }
            set { _Settings._AllowAutoUpdate = value; }
        }

        public static bool AllowAutoUpdate
        {
            get
            {
                if (_Settings._AllowAutoUpdate == null)
                {
                    return true;
                }
                return (bool) _Settings._AllowAutoUpdate;
            }
            set { _Settings._AllowAutoUpdate = value; }
        }

        public static string Language
        {
            get { return _Settings._Language; }
            set { _Settings._Language = value; }
        }

        #endregion Accessor

        private static readonly string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string fileName = "Settings.xml";
        private static readonly string filePath = Path.Combine(appPath, fileName);

        internal static string ConsumerKey
        {
            get { return "WY93gCVpkgAMdVAVuu5yDYB0g"; }
        }

        internal static string ConsumerSecret
        {
            get { return "Q1GYjZd9F78NdbpCStl9u0lbMcqJGmJjqXnZS9NQqddMyqwS5t"; }
        }

        /// <summary>
        ///     設定を読み込むのに必要なシード値を生成し、設定の読み込みを試行する
        /// </summary>
        /// <returns>設定を読み込めたかどうか</returns>
        public static bool Initialize()
        {
            string mn = Environment.MachineName;
            string un = Environment.UserName;
            string udn = Environment.UserDomainName;

            byte[] rawHash = Crypt.CreateSeed(mn + un + udn);
            byte[] hash = Crypt.CreateSeed(rawHash);

            ReadSettings();

            if (File.Exists(filePath) && Hash != null)
            {
                if (Hash.SequenceEqual(hash))
                {
                    return true;
                }
            }

            UserName = "";
            Password = "";
            AccessToken = "";
            AccessTokenSecret = "";

            Hash = hash;

            return false;
        }

        /// <summary>
        ///     ファイルから設定を読み込む
        /// </summary>
        private static void ReadSettings()
        {
            var xmls = new XMLSettings();
            var xs = new XmlSerializer(typeof (XMLSettings));
            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    xmls = (XMLSettings) xs.Deserialize(fs);
                    fs.Close();
                }
            }

            _Settings._Hash = TryReadValue(xmls.Hash, null, null);

            _Settings._PrimaryServerUrl = TryReadValue(xmls.PrimaryServerUrl, null, null);
            _Settings._SecondaryServerUrl = TryReadValue(xmls.SecondaryServerUrl, null, null);
            _Settings._IgnoreSSLcertificateError = xmls.IgnoreSSLcertificateError;
            _Settings._UserName = TryReadValue(xmls.UserName, null, null);
            _Settings._Password = TryReadValue(xmls.Password, null, null);

            _Settings._NetworkBuffer = TryReadValue(xmls.NetworkBuffer, 0, 10);
            _Settings._TargetBitrate = TryReadValue(xmls.TargetBitrate, 0, 128);

            _Settings._AudioMethod = xmls.AudioMethod;
            _Settings._AudioDevice = TryReadValue(xmls.AudioDevice, null, "Default");
            _Settings._AudioBuffer = TryReadValue(xmls.AudioBuffer, 0, 250);
            _Settings._Volume = xmls.Volume;
            _Settings._RepeatState = TryReadValue(xmls.RepeatState, null, 0);

            _Settings._TweetTextFormat = TryReadValue(xmls.TweetTextFormat, null,
                "NowPlaying: %title% - %artist% / %album%");
            _Settings._TweetUrl = xmls.TweetUrl;
            _Settings._AccessToken = xmls.AccessToken;
            _Settings._AccessTokenSecret = xmls.AccessTokenSecret;

            _Settings._AllowUpdateCheck = xmls.AllowUpdateCheck;
            _Settings._AllowAutoUpdate = xmls.AllowAutoUpdate;

            _Settings._Language = xmls.Language;
        }

        private static dynamic TryReadValue(dynamic source, dynamic check, dynamic defaultValue)
        {
            if (source != check)
            {
                return source;
            }
            return defaultValue;
        }

        /// <summary>
        ///     ファイルへ設定を書き込む
        /// </summary>
        public static void WriteSettings()
        {
            var xmls = new XMLSettings
            {
                Hash = _Settings._Hash,
                PrimaryServerUrl = _Settings._PrimaryServerUrl,
                SecondaryServerUrl = _Settings._SecondaryServerUrl,
                IgnoreSSLcertificateError = _Settings._IgnoreSSLcertificateError,
                UserName = _Settings._UserName,
                Password = _Settings._Password,
                NetworkBuffer = _Settings._NetworkBuffer,
                TargetBitrate = _Settings._TargetBitrate,
                AudioMethod = _Settings._AudioMethod,
                AudioDevice = _Settings._AudioDevice,
                AudioBuffer = _Settings._AudioBuffer,
                Volume = _Settings._Volume,
                RepeatState = _Settings._RepeatState,
                TweetTextFormat = _Settings._TweetTextFormat,
                TweetUrl = _Settings._TweetUrl,
                AccessToken = _Settings._AccessToken,
                AccessTokenSecret = _Settings._AccessTokenSecret,
                AllowUpdateCheck = AllowUpdateCheck,
                AllowAutoUpdate = AllowAutoUpdate,
                Language = Language
            };

            var xs = new XmlSerializer(typeof (XMLSettings));
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, xmls);
            }
        }
    }
}