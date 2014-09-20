using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Mánagarmr.Models.SubsonicAPI
{
    class GetCoverArt
    {
        private string APIuri { get { return "rest/getCoverArt.view"; } }

        private MemoryStream ms;

        public MemoryStream GetCoverArtImage(string songId)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName + "&size=500&id=" + songId;

            GetBitmap(url);
            if (ms != null)
            {
                return ms;
            }
            else
            {
                return null;
            }
        }

        private void GetBitmap(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(Settings.UserName, Settings.Password);

            byte[] data = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    data = wc.DownloadData(url);
                    ms = new MemoryStream(data);

                    break;
                }
                catch { }
            }
        }
    }
}
