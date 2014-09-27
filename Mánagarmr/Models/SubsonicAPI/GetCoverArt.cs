using System.IO;
using System.Net;

namespace Mánagarmr.Models.SubsonicAPI
{
    internal class GetCoverArt
    {
        private MemoryStream _ms;

        private string APIuri
        {
            get { return "rest/getCoverArt.view"; }
        }

        public MemoryStream GetCoverArtImage(string songId)
        {
            string url = APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName +
                         "&size=500&id=" + songId;

            GetBitmap(url);
            if (_ms != null)
            {
                return _ms;
            }
            return null;
        }

        public string GetCoverArtImageUrl(string songId)
        {
            return APIhelper.Url + APIuri + "?v=" + APIhelper.ApiVersion + "&c=" + APIhelper.AppName + "&u=" +
                   Settings.UserName + "&p=enc:" + APIhelper.GenerateHexEncodedPassword(Settings.Password) +
                   "&size=500&id=" + songId;
        }

        private void GetBitmap(string url)
        {
            var wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = APIhelper.BuildBasicAuthString(Settings.UserName,
                Settings.Password);

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    byte[] data = wc.DownloadData(url);
                    _ms = new MemoryStream(data);

                    break;
                }
                catch
                {
                }
            }
        }
    }
}