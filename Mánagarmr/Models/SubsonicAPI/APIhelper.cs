using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    public static class APIhelper
    {
        public static System.Net.Security.RemoteCertificateValidationCallback defaultSSLcartificateCallBack = ServicePointManager.ServerCertificateValidationCallback;
        public static string apiVersion { get { return "1.8.0"; } }
        public static string appName { get { return "Managarmr"; } }
        public static string url { get; private set; }

        public static StreamInfoPack sip { get; set; }
        public static MemoryStream ms { get; set; }
        public static Dictionary<int, FolderListInfoPack> flipd { get; set; }
        public static Dictionary<int, LibraryListInfoPack> llipd { get; set; }

        public static string BuildBasicAuthString(string userName, string password)
        {
            var AuthBase = userName + ":" + password;
            var chars = Encoding.ASCII.GetBytes(AuthBase);
            var base64 = Convert.ToBase64String(chars);
            return "Basic " + base64;
        }

        public static void BuildBaseUrl()
        {
            var p = new Ping();
            bool result = false;

            if (String.IsNullOrEmpty(Settings.PrimaryServerUrl) == false)
            {
                var testUrl = Settings.PrimaryServerUrl;

                if (testUrl.EndsWith("/") == false)
                {
                    testUrl = testUrl + "/";
                }
                p.CheckServer(testUrl, Settings.IgnoreSSLcertificateError, Settings.UserName, Settings.Password, out result);
                if (result == true)
                {
                    Debug.WriteLine("ready to connect primary server");
                    url = testUrl;
                    return;
                }
            }

            if (String.IsNullOrEmpty(Settings.SecondaryServerUrl) == false)
            {
                var testUrl = Settings.SecondaryServerUrl;

                if (testUrl.EndsWith("/") == false)
                {
                    testUrl = testUrl + "/";
                }
                p.CheckServer(testUrl, Settings.IgnoreSSLcertificateError, Settings.UserName, Settings.Password, out result);
                if (result == true)
                {
                    Debug.WriteLine("ready to connect secondery server");
                    url = testUrl;
                    return;
                }
            }
        }

        public static void TryParseXML(XmlDocument doc, string tag, string attr, out List<string> value)
        {
            value = new List<string>();
            XmlNodeList nodes = doc.GetElementsByTagName(tag);

            if (nodes == null)
            {
                value = null;
                return;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlElement elm = nodes[i] as XmlElement;
                string str = elm.GetAttribute(attr);
                if (str != null)
                {
                    value.Add(str);
                }
            }
        }
    }
}
