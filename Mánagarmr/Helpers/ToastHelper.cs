using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Mánagarmr.Views;

namespace Mánagarmr.Helpers
{
    public static class ToastHelper
    {
        private static Toast toast;

        public static string Album { get; set; }
        public static string Title { get; set; }
        public static string Artist { get; set; }
        public static string AlbumCoverImageUrl { get; set; }

        public static void Dispose()
        {
            if (toast != null)
            {
                try
                {
                    toast.Close();
                }
                catch { }
            }
            toast = null;
        }

        public static void ShowToastDispatcher(string title, string album, string artist, string albumCoverImageUrl)
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                ShowToast(title, album, artist, albumCoverImageUrl);
            }
            else
            {
                dispatcher.Invoke(() => ShowToast(title, album, artist, albumCoverImageUrl));
            }
        }

        private static void ShowToast(string title, string album, string artist, string albumCoverImageUrl)
        {
            Album = album;
            Title = title;
            Artist = artist;
            AlbumCoverImageUrl = albumCoverImageUrl;

            if (toast != null)
            {
                Dispose();
            }
            toast = new Toast();

            var desktop = Screen.PrimaryScreen.WorkingArea;
            toast.Top = desktop.Height - (toast.Height + 8);
            toast.Left = desktop.Width - (toast.Width + 8);
            toast.ShowActivated = false;
            toast.Show();
        }
    }
}
