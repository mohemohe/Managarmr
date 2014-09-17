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
    class GetSong
    {
        private static string APIuri { get { return "rest/getSong.view"; } }

        public void GetSongInfo(string songId, out StreamInfoPack result)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName + "&id=" + songId;
            var sip = new StreamInfoPack();

            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(Settings.UserName, Settings.Password);

            byte[] data = null;
            try
            {
                data = wc.DownloadData(url);
            }
            catch
            {
                sip.status = "error";
                result = sip;
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
                sip.status = "error";
                result = sip;
                return;
            }

            List<string> list;
            APIhelper.TryParseXML(doc, "subsonic-response", "status", out list);
            if (list[0] != "ok")
            {
                sip.status = "error";
                result = sip;
                return;
            }
            sip.status = list[0];
            APIhelper.TryParseXML(doc, "song", "id", out list);
            sip.id = list[0];
            APIhelper.TryParseXML(doc, "song", "parent", out list);
            sip.parent = list[0];
            APIhelper.TryParseXML(doc, "song", "title", out list);
            sip.title = list[0];
            APIhelper.TryParseXML(doc, "song", "album", out list);
            sip.album = list[0];
            APIhelper.TryParseXML(doc, "song", "artist", out list);
            sip.artist = list[0];
            APIhelper.TryParseXML(doc, "song", "coverArt", out list);
            sip.coverArt = list[0];
            APIhelper.TryParseXML(doc, "song", "duration", out list);
            sip.duration = list[0];

            result = sip;
        }
    }
}
