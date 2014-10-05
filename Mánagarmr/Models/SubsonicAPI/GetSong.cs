using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class GetSong
    {
        private string _xmlBody;

        private string APIuri
        {
            get { return "rest/getSong.view"; }
        }

        public StreamInfoPack GetSongInfo(string songId)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName + "&id=" +
                         songId;

            try
            {
                GetXMLbody(url);
            }
            catch
            {
            }

            if (_xmlBody == null)
            {
                var sip = new StreamInfoPack();
                sip.Status = "error";
                return sip;
            }

            return ParseXML(_xmlBody);
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

        private StreamInfoPack ParseXML(string stringDoc)
        {
            var gc = new GetCoverArt();
            var sip = new StreamInfoPack();
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(stringDoc);
            }
            catch
            {
                sip.Status = "error";
                return sip;
            }

            List<string> list;
            APIhelper.TryParseXML(xmlDoc, "subsonic-response", "status", out list);

            if (list[0] != "ok")
            {
                sip.Status = "error";
                return sip;
            }
            sip.Status = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "id", out list);
            sip.Id = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "parent", out list);
            sip.Parent = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "title", out list);
            sip.Title = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "album", out list);
            sip.Album = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "artist", out list);
            sip.Artist = list[0];
            APIhelper.TryParseXML(xmlDoc, "song", "coverArt", out list);
            sip.CoverArt = gc.GetCoverArtImageUrl(list[0]);
            APIhelper.TryParseXML(xmlDoc, "song", "duration", out list);
            sip.Duration = list[0];

            return sip;
        }
    }
}