using System;

namespace Mánagarmr.Models.SubsonicAPI.InfoPack
{
    public class LibraryListInfoPack
    {
        public string Title { get; set; }

        public string ID { get; set; }

        public bool IsDir { get; set; }

        public string Track { get; set; }

        public string Artist { get; set; }

        public string AlbumId { get; set; }

        public string CoverArtUrl { get; set; }

        public string Album { get; set; }
    }
}