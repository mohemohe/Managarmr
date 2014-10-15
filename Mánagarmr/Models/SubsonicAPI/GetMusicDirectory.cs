using System;
using Mánagarmr.Models.SubsonicAPI.InfoPack;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class GetMusicDirectory
    {
        private List<string> _parentId;
        private List<string> _dirName; 
        private List<string> _albumId;
        private List<string> _artist;
        private List<string> _id;
        private List<string> _isDir;
        private List<string> _title;
        private List<string> _track;
        private string _xmlBody;

        private static string APIuri
        {
            get { return "rest/getMusicDirectory.view"; }
        }

        public Dictionary<int, LibraryListInfoPack> GetMusicDir(string libId)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName + "&id=" +
                         libId;

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
                if (String.IsNullOrEmpty(_parentId[0]))
                {
                    for (int i = 0; i < _id.Count; i++)
                    {
                        if (_title[i] != "Scans" && _title[i] != "Scan")
                        {
                            llipd.Add(i, new LibraryListInfoPack
                            {
                                ID = _id[i],
                                Title = _title[i],
                                Track = _track[i],
                                Artist = _artist[i],
                                //Album = _albumId[i],
                                AlbumId = _albumId[i],
                                IsDir = Convert.ToBoolean(_isDir[i])
                            });
                        }
                    }
                }
                else
                {
                    llipd.Add(0, new LibraryListInfoPack
                    {
                        ID = _parentId[0],
                        Title = "...",
                        AlbumId = _parentId[0],
                        IsDir = true
                    });

                    for (int i = 0; i < _id.Count; i++)
                    {
                        if (_title[i] != "Scans" && _title[i] != "Scan")
                        {
                            llipd.Add(i + 1, new LibraryListInfoPack
                            {
                                ID = _id[i],
                                Title = _title[i],
                                Track = _track[i],
                                Artist = _artist[i],
                                //Album = _albumId[i],
                                AlbumId = _albumId[i],
                                IsDir = Convert.ToBoolean(_isDir[i])
                            });
                        }
                    }
                }
            }
            return llipd;
        }

        public string GetMusicDirName(string libId)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName + "&id=" +
                         libId;

            try
            {
                GetXMLbody(url);
            }
            catch
            {
            }

            ParseXML(_xmlBody);
            return _dirName[0];
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

            APIhelper.TryParseXML(xmlDoc, "directory", "parent", out _parentId);
            APIhelper.TryParseXML(xmlDoc, "directory", "name", out _dirName);
            APIhelper.TryParseXML(xmlDoc, "child", "title", out _title);
            APIhelper.TryParseXML(xmlDoc, "child", "id", out _id);
            APIhelper.TryParseXML(xmlDoc, "child", "track", out _track);
            APIhelper.TryParseXML(xmlDoc, "child", "artist", out _artist);
            APIhelper.TryParseXML(xmlDoc, "child", "isDir", out _isDir);
            APIhelper.TryParseXML(xmlDoc, "child", "albumId", out _albumId);
        }
    }
}