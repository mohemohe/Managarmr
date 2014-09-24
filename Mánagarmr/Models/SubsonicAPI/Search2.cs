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
    class Search2
    {
        private static string APIuri { get { return "rest/search2.view"; } }

        private string xmlBody;
        public List<string> albumTitle;
        public List<string> albumId;
        public List<string> albumTrack;
        public List<string> albumArtist;
        public List<string> albumIsDir;
        public List<string> songTitle;
        public List<string> songId;
        public List<string> songTrack;
        public List<string> songArtist;
        public List<string> songIsDir;


        public Dictionary<int, LibraryListInfoPack> Search(string query)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName + "&artistCount=" + Int32.MaxValue.ToString() + "&albumCount=" + Int32.MaxValue.ToString() + "&songCount=" + Int32.MaxValue.ToString() + "&query=" + query;

            try
            {
                GetXMLbody(url);
            }
            catch { }

            ParseXML(xmlBody);

            var llipd = new Dictionary<int, LibraryListInfoPack>();
            int i = 0;

            if (xmlBody != null && albumId != null)
            {
                do
                {
                    try
                    {
                        llipd.Add(i, new LibraryListInfoPack(albumId[i], albumTitle[i], albumTrack[i], albumArtist[i], albumIsDir[i]));
                        i++;
                    }
                    catch { }
                } while (i < albumId.Count);
            }

            if (xmlBody != null && songId != null)
            {
                do
                {
                    try
                    {
                        llipd.Add(i, new LibraryListInfoPack(songId[i], songTitle[i], songTrack[i], songArtist[i], songIsDir[i]));
                        i++;
                    }
                    catch { }
                } while (i < songId.Count);
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

            APIhelper.TryParseXML(xmlDoc, "album", "title", out albumTitle);
            APIhelper.TryParseXML(xmlDoc, "album", "id", out albumId);
            APIhelper.TryParseXML(xmlDoc, "album", "track", out albumTrack);
            APIhelper.TryParseXML(xmlDoc, "album", "artist", out albumArtist);
            APIhelper.TryParseXML(xmlDoc, "album", "isDir", out albumIsDir);

            APIhelper.TryParseXML(xmlDoc, "song", "title", out songTitle);
            APIhelper.TryParseXML(xmlDoc, "song", "id", out songId);
            APIhelper.TryParseXML(xmlDoc, "song", "track", out songTrack);
            APIhelper.TryParseXML(xmlDoc, "song", "artist", out songArtist);
            APIhelper.TryParseXML(xmlDoc, "song", "isDir", out songIsDir);
        }
    }
}
