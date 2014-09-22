using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mánagarmr.Models.SubsonicAPI.InfoPack
{
    public class FolderListInfoPack
    {
        public string name { get; set; }
        public string id { get; set; }

        public FolderListInfoPack(string id, string name)
        {
            this.name = name;
            this.id = id;
        }
    }
}
