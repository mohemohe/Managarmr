using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    class GetMusicDirectory
    {
        private static string APIuri { get { return "rest/getMusicDirectory.view"; } }

        private string xmlBody;
        public List<string> title;
        public List<string> id;
        public List<string> isDir;


        public Dictionary<int, LibraryListInfoPack> GetMusicDir(string libId)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName + "&id=" + libId;

            try
            {
                GetXMLbody(url);
            }
            catch { }

            ParseXML(xmlBody);

            var llipd = new Dictionary<int, LibraryListInfoPack>();

            if (xmlBody != null && id != null)
            {
                for (int i = 0; i < id.Count; i++)
                {
                    if (title[i] != "Scans" && title[i] != "Scan")
                    {
                        llipd.Add(i, new LibraryListInfoPack(id[i], title[i], isDir[i]));
                    }
                }
            }
            return llipd;
        }

        private void GetXMLbody(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(Settings.UserName, Settings.Password);

            xmlBody = null;
            byte[] data = null;
            try
            {
                data = wc.DownloadData(url);
            }
            catch
            {
                return;
            }

            if (data != null)
            {
                var enc = Encoding.GetEncoding("UTF-8");
                try
                {
                    xmlBody = enc.GetString(data);
                }
                catch { }
            }
        }

        private void ParseXML(string stringDoc)
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(stringDoc);
            }
            catch
            {
                return;
            }

            List<string> list;
            APIhelper.TryParseXML(xmlDoc, "subsonic-response", "status", out list);

            if (list[0] != "ok")
            {
                return;
            }

            APIhelper.TryParseXML(xmlDoc, "child", "title", out title);
            APIhelper.TryParseXML(xmlDoc, "child", "id", out id);
            APIhelper.TryParseXML(xmlDoc, "child", "isDir", out isDir);
        }
    }
}
