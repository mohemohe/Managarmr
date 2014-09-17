using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    class Ping
    {
        public void CheckServer(string url, bool ignoreSSLcertificateError, string userName, string password, out bool result)
        {
            ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, errors) => ignoreSSLcertificateError;

            url = url + "rest/ping.view?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName;

            result = false;

            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(userName, password);

            byte[] data;
            try
            {
                data = wc.DownloadData(url);
            }
            catch
            {
                result = false;
                return;
            }

            var enc = Encoding.GetEncoding("UTF-8");
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(enc.GetString(data));
            }
            catch
            {
                return;
            }

            List<string> statusList;
            APIhelper.TryParseXML(doc, "subsonic-response", "status", out statusList);

            foreach (var status in statusList)
            {
                if (status == "ok")
                {
                    result = true;
                }
            }
        }
    }
}
