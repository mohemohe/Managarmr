using Rhinemaidens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mánagarmr.Models
{
    class Twitter
    {
        Lorelei lorelei = new Lorelei();
        private string consumerKey { get { return "WY93gCVpkgAMdVAVuu5yDYB0g"; } }
        private string consumerSecret { get { return "Q1GYjZd9F78NdbpCStl9u0lbMcqJGmJjqXnZS9NQqddMyqwS5t"; } }

        public Twitter()
        {
            if (String.IsNullOrEmpty(Settings.AccessToken) == false && String.IsNullOrEmpty(Settings.AccessTokenSecret) == false)
            {
                lorelei.Initialize(consumerKey, consumerSecret, Settings.AccessToken, Settings.AccessTokenSecret);
            }
        }

        public void ReInitialize()
        {
            lorelei.Initialize(Settings.AccessToken, Settings.AccessTokenSecret);
        }

        public void ReInitialize(string accessToken, string accessTokenSecret)
        {
            lorelei.Initialize(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        public void Tweet(string title, string artist, string album)
        {
            var tweetBody = GenerateTweetBody(title, artist, album);
            lorelei.PostTweet(tweetBody);
        }

        private string GenerateTweetBody(string title, string artist, string album)
        {
            return Settings.TweetTextFormat.Replace("%title%", title)
                                           .Replace("%artist%", artist)
                                           .Replace("%album%", album);
        }
    }
}
