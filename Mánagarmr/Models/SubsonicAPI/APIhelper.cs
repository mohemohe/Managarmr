using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    public static class APIhelper
    {
        public static RemoteCertificateValidationCallback DefaultSsLcartificateCallBack =
            ServicePointManager.ServerCertificateValidationCallback;

        public static string ApiVersion
        {
            get { return "1.8.0"; }
        }

        public static string AppName
        {
            get { return "Managarmr"; }
        }

        public static string Url { get; private set; }

        public static StreamInfoPack Sip { get; set; }

        public static MemoryStream Ms { get; set; }

        public static Dictionary<int, FolderListInfoPack> Flipd { get; set; }

        public static Dictionary<int, LibraryListInfoPack> Llipd { get; set; }

        public static Dictionary<int, LibraryListInfoPack> Header { get; set; }

        public static string BuildBasicAuthString(string userName, string password)
        {
            string authBase = userName + ":" + password;
            byte[] chars = Encoding.ASCII.GetBytes(authBase);
            string base64 = Convert.ToBase64String(chars);
            return "Basic " + base64;
        }

        public static string GenerateHexEncodedPassword(string basePassword)
        {
            byte[] data = Encoding.UTF8.GetBytes(Settings.Password);
            string hexText = BitConverter.ToString(data);
            return hexText.Replace("-", "");
        }

        public static void BuildBaseUrl()
        {
            var p = new Ping();
            bool result;

            if (String.IsNullOrEmpty(Settings.PrimaryServerUrl) == false)
            {
                string testUrl = Settings.PrimaryServerUrl;

                if (testUrl.EndsWith("/") == false)
                {
                    testUrl = testUrl + "/";
                }
                p.CheckServer(testUrl, Settings.IgnoreSSLcertificateError, Settings.UserName, Settings.Password,
                    out result);
                if (result)
                {
                    Debug.WriteLine("ready to connect primary server");
                    Url = testUrl;
                    return;
                }
            }

            if (String.IsNullOrEmpty(Settings.SecondaryServerUrl) == false)
            {
                string testUrl = Settings.SecondaryServerUrl;

                if (testUrl.EndsWith("/") == false)
                {
                    testUrl = testUrl + "/";
                }
                p.CheckServer(testUrl, Settings.IgnoreSSLcertificateError, Settings.UserName, Settings.Password,
                    out result);
                if (result)
                {
                    Debug.WriteLine("ready to connect secondery server");
                    Url = testUrl;
                }
            }
        }

        public static void TryParseXML(XmlDocument doc, string tag, string attr, out List<string> value)
        {
            value = new List<string>();
            XmlNodeList nodes = doc.GetElementsByTagName(tag);

            for (int i = 0; i < nodes.Count; i++)
            {
                var elm = nodes[i] as XmlElement;
                if (elm != null)
                {
                    string str = elm.GetAttribute(attr);
                    value.Add(str);
                }
            }
        }
    }
}