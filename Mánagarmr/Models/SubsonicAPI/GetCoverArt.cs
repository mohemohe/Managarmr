using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Mánagarmr.Models.SubsonicAPI
{
    class GetCoverArt
    {
        private static string APIuri { get { return "rest/getCoverArt.view"; } }

        public void GetCoverArtImage(string songId, out BitmapSource image)
        {
            var url = APIhelper.url + APIuri + "?v=" + APIhelper.apiVersion + "&c=" + APIhelper.appName + "&size=500&id=" + songId;
            image = null;

            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(Settings.UserName, Settings.Password);

            byte[] data = null;
            try
            {
                data = wc.DownloadData(url);
                
                var ms = new MemoryStream(data);
                var bitmap = new Bitmap(ms);
                var hBitmap = bitmap.GetHbitmap();

                image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        hBitmap,
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            catch { }
        }
    }
}
