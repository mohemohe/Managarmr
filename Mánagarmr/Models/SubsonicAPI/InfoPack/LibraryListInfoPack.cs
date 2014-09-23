using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mánagarmr.Models.SubsonicAPI.InfoPack
{
    public class LibraryListInfoPack
    {
        public string title { get; set; }
        public string id { get; set; }
        public bool isDir { get; set; }
        public string track { get; set; }
        public string artist { get; set; }

        public LibraryListInfoPack(string id, string title)
        {
            this.title = title;
            this.id = id;
        }

        public LibraryListInfoPack(string id, string title, string isDir)
        {
            this.title = title;
            this.id = id;
            this.isDir = Convert.ToBoolean(isDir);
        }
        
        public LibraryListInfoPack(string id, string title, string track, string artist, string isDir)
        {
            this.title = title;
            this.id = id;
            this.track = track;
            this.artist = artist;
            this.isDir = Convert.ToBoolean(isDir);
        }
    }
}
