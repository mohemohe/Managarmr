using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class Ping
    {
// ReSharper disable once InconsistentNaming
        public void CheckServer(string url, bool ignoreSSLcertificateError, string userName, string password,
            out bool result)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, cert, chain, errors) => ignoreSSLcertificateError;

            url = url + "rest/ping.view?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName;

            result = false;

            WebRequest wr = WebRequest.Create(url);
            wr.Timeout = 5000;
            wr.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(userName, password);

            string str = null;

            try
            {
                using (WebResponse rs = wr.GetResponse())
                using (System.IO.Stream grs = rs.GetResponseStream())
                {
                    if (grs != null)
                    {
                        var reader = new StreamReader(grs);
                        str = reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                return;
            }

            var doc = new XmlDocument();
            try
            {
                if (str != null) doc.LoadXml(str);
            }
            catch
            {
                return;
            }

            List<string> statusList;
            APIhelper.TryParseXML(doc, "subsonic-response", "status", out statusList);

// ReSharper disable once UnusedVariable
            foreach (string status in statusList.Where(status => status == "ok"))
            {
                result = true;
            }
        }
    }
}