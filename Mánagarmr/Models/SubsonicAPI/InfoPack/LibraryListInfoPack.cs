using System;

namespace Mánagarmr.Models.SubsonicAPI.InfoPack
{
    public class LibraryListInfoPack
    {
        public LibraryListInfoPack(string id, string title)
        {
            Title = title;
            Id = id;
        }

        public LibraryListInfoPack(string id, string title, string isDir)
        {
            Title = title;
            Id = id;
            IsDir = Convert.ToBoolean(isDir);
        }

        public LibraryListInfoPack(string id, string title, string track, string artist, string isDir)
        {
            Title = title;
            Id = id;
            Track = track;
            Artist = artist;
            IsDir = Convert.ToBoolean(isDir);
        }

        public LibraryListInfoPack(string id, string title, string track, string artist, string albumId, string isDir)
        {
            Title = title;
            Id = id;
            Track = track;
            Artist = artist;
            AlbumId = albumId;
            IsDir = Convert.ToBoolean(isDir);
        }

        public LibraryListInfoPack(string id, string title, string track, string artist, string albumId, string isDir,
            string coverArtUrl)
        {
            Title = title;
            Id = id;
            Track = track;
            Artist = artist;
            AlbumId = albumId;
            IsDir = Convert.ToBoolean(isDir);
            CoverArtUrl = coverArtUrl;
        }

        public string Title { get; set; }

        public string Id { get; set; }

        public bool IsDir { get; set; }

        public string Track { get; set; }

        public string Artist { get; set; }

        public string AlbumId { get; set; }

        public string CoverArtUrl { get; set; }
    }
}