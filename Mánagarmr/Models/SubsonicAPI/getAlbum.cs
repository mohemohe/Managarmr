using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class GetAlbum
    {
        private readonly GetCoverArt _gca = new GetCoverArt();
        private List<string> _artist;
        private List<string> _coverArt;
        private List<string> _id;
        private List<string> _title;
        private string _xmlBody;

        private static string APIuri
        {
            get { return "rest/getAlbum.view"; }
        }

        public Dictionary<int, LibraryListInfoPack> GetAlbumInfo(string albumId)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName + "&id=" +
                         albumId;

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
                    llipd.Add(i, new LibraryListInfoPack{
                            ID = _id[i], 
                            Title = _title[i],
                            Artist = _artist[i],
                            CoverArtUrl = _gca.GetCoverArtImageUrl(_coverArt[i])
                        });
                }
            }
            return llipd;
        }

        // ReSharper disable once InconsistentNaming
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

            APIhelper.TryParseXML(xmlDoc, "album", "name", out _title);
            APIhelper.TryParseXML(xmlDoc, "album", "id", out _id);
            APIhelper.TryParseXML(xmlDoc, "album", "artist", out _artist);
            APIhelper.TryParseXML(xmlDoc, "album", "coverArt", out _coverArt);
        }
    }
}