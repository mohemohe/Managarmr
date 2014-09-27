using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    public class GetIndexes
    {
        public List<string> Id;
        public List<string> Name;
        private string _xmlBody;

        private static string APIuri
        {
            get { return "rest/getIndexes.view"; }
        }

        public Dictionary<int, FolderListInfoPack> GetIndex()
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName;

            try
            {
                GetXMLbody(url);
            }
            catch
            {
            }

            ParseXML(_xmlBody);

            var flipd = new Dictionary<int, FolderListInfoPack>();

            if (_xmlBody != null)
            {
                for (int i = 0; i < Id.Count; i++)
                {
                    flipd.Add(i, new FolderListInfoPack(Id[i], Name[i]));
                }
            }
            return flipd;
        }

        private void GetXMLbody(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(Settings.UserName,
                Settings.Password);

            _xmlBody = null;
            byte[] data;
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
                Encoding enc = Encoding.GetEncoding("UTF-8");
                try
                {
                    _xmlBody = enc.GetString(data);
                }
                catch
                {
                }
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

            APIhelper.TryParseXML(xmlDoc, "artist", "name", out Name);
            APIhelper.TryParseXML(xmlDoc, "artist", "id", out Id);
        }
    }
}