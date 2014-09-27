using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    public class GetAlbumList
    {
        private List<string> _artist;
        private List<string> _id;
        private List<string> _isDir;
        private List<string> _title;
        private List<string> _track;
        private string _xmlBody;

        private static string APIuri
        {
            get { return "rest/getAlbumList.view"; }
        }

        public Dictionary<int, LibraryListInfoPack> GetRandomAlbumList()
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName +
                         "&type=random&size=16";

            try
            {
                GetXMLbody(url);
            }
            catch
            {
            }

            ParseXML(_xmlBody);

            var llipd = new Dictionary<int, LibraryListInfoPack>();

            if (_xmlBody != null && _id != null)
            {
                for (int i = 0; i < _id.Count; i++)
                {
                    llipd.Add(i, new LibraryListInfoPack(_id[i], _title[i], _track[i], _artist[i], _isDir[i]));
                }
            }
            return llipd;
        }

        public Dictionary<int, LibraryListInfoPack> GetNewestAlbumList()
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName +
                         "&type=newest&size=16";

            try
            {
                GetXMLbody(url);
            }
            catch
            {
            }

            ParseXML(_xmlBody);

            var llipd = new Dictionary<int, LibraryListInfoPack>();

            if (_xmlBody != null && _id != null)
            {
                for (int i = 0; i < _id.Count; i++)
                {
                    llipd.Add(i, new LibraryListInfoPack(_id[i], _title[i], _track[i], _artist[i], _isDir[i]));
                }
            }
            return llipd;
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

            APIhelper.TryParseXML(xmlDoc, "album", "title", out _title);
            APIhelper.TryParseXML(xmlDoc, "album", "id", out _id);
            APIhelper.TryParseXML(xmlDoc, "album", "track", out _track);
            APIhelper.TryParseXML(xmlDoc, "album", "artist", out _artist);
            APIhelper.TryParseXML(xmlDoc, "album", "isDir", out _isDir);
        }
    }
}