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
    public class GetIndexes
    {
        private static string APIuri { get { return "rest/getIndexes.view"; } }

        private string xmlBody;
        public List<string> name;
        public List<string> id;


        public Dictionary<int, FolderListInfoPack> GetIndex()
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName;

            try
            {
                GetXMLbody(url);
            }
            catch { }

            ParseXML(xmlBody);

            var flipd = new Dictionary<int, FolderListInfoPack>();

            if (xmlBody != null)
            {
                for (int i = 0; i < id.Count; i++)
                {
                    flipd.Add(i, new FolderListInfoPack(id[i], name[i]));
                }
            }
            return flipd;
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

            APIhelper.TryParseXML(xmlDoc, "artist", "name", out name);
            APIhelper.TryParseXML(xmlDoc, "artist", "id", out id);
        }
    }
}
