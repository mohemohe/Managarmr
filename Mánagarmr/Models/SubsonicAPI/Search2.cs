using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class Search2
    {
        private List<string> _albumArtist;
        private List<string> _albumId;
        private List<string> _albumIsDir;
        private List<string> _albumTitle;
        private List<string> _albumTrack;
        private List<string> _songArtist;
        private List<string> _songId;
        private List<string> _songIsDir;
        private List<string> _songTitle;
        private List<string> _songTrack;
        private string _xmlBody;

        private static string APIuri
        {
            get { return "rest/search2.view"; }
        }

        public Dictionary<int, LibraryListInfoPack> Search(string query)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName +
                         "&artistCount=" + Int32.MaxValue + "&albumCount=" + Int32.MaxValue + "&songCount=" +
                         Int32.MaxValue + "&query=" + query;

            try
            {
                GetXMLbody(url);
            }
            catch
            {
            }

            ParseXML(_xmlBody);

            var llipd = new Dictionary<int, LibraryListInfoPack>();
            int i = 0;

            if (_xmlBody != null && _albumId != null)
            {
                do
                {
                    try
                    {
                        llipd.Add(i, new LibraryListInfoPack{
                            ID = _albumId[i],
                            Title = _albumTitle[i],
                            Track = _albumTrack[i],
                            Artist = _albumArtist[i],
                            IsDir = Convert.ToBoolean(_albumIsDir[i])
                        });
                        i++;
                    }
                    catch
                    {
                    }
                } while (i < _albumId.Count);
            }

            if (_xmlBody != null && _songId != null)
            {
                do
                {
                    try
                    {
                        llipd.Add(i, new LibraryListInfoPack
                        {
                            ID = _songId[i],
                            Title = _songTitle[i],
                            Track = _songTrack[i],
                            Artist = _songArtist[i],
                            IsDir = Convert.ToBoolean(_songIsDir[i])
                        });
                        i++;
                    }
                    catch
                    {
                    }
                } while (i < _songId.Count);
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

            APIhelper.TryParseXML(xmlDoc, "album", "title", out _albumTitle);
            APIhelper.TryParseXML(xmlDoc, "album", "id", out _albumId);
            APIhelper.TryParseXML(xmlDoc, "album", "track", out _albumTrack);
            APIhelper.TryParseXML(xmlDoc, "album", "artist", out _albumArtist);
            APIhelper.TryParseXML(xmlDoc, "album", "isDir", out _albumIsDir);

            APIhelper.TryParseXML(xmlDoc, "song", "title", out _songTitle);
            APIhelper.TryParseXML(xmlDoc, "song", "id", out _songId);
            APIhelper.TryParseXML(xmlDoc, "song", "track", out _songTrack);
            APIhelper.TryParseXML(xmlDoc, "song", "artist", out _songArtist);
            APIhelper.TryParseXML(xmlDoc, "song", "isDir", out _songIsDir);
        }
    }
}