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
        private string APIuri { get { return "rest/getSong.view"; } }

        private string xmlBody;

        public StreamInfoPack GetSongInfo(string songId)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName + "&id=" + songId;

            try
            {
                GetXMLbody(url);
            }
            catch { }

            if (xmlBody == null)
            {
                var sip = new StreamInfoPack();
                sip.status = "error";
                return sip;
            }

            return ParseXML(xmlBody);
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

        private StreamInfoPack ParseXML(string stringDoc)
        {
            var sip = new StreamInfoPack();
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(stringDoc);
            }
            catch
            {
                sip.status = "error";
                return sip;
            }

            List<string> list;
            APIhelper.TryParseXML(xmlDoc, "subsonic-response", "status", out list);

            if (list[0] != "ok")
            {
                sip.status = "error";
                return sip;
            }
            sip.status = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "id", out list);
            sip.id = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "parent", out list);
            sip.parent = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "title", out list);
            sip.title = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "album", out list);
            sip.album = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "artist", out list);
            sip.artist = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "coverArt", out list);
            sip.coverArt = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "duration", out list);
            sip.duration = list[0];

            return sip;
        }
    }
}
