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
            lorelei.Initialize(consumerKey, consumerSecret);
        }
        public Twitter(string accessToken, string accessTokenSecret)
        {
            lorelei.Initialize(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        public void Initialize(string accessToken, string accessTokenSecret)
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
            var tweetBody = Settings.TweetTextFormat;

            if (tweetBody.Contains("%title%"))
            {
                tweetBody = tweetBody.Replace("%title%", title);
            }
            else
            {
                tweetBody = tweetBody.Replace("%title%", "");
            }
            if (tweetBody.Contains("%artist%"))
            {
                tweetBody = tweetBody.Replace("%artist%", artist);
            }
            else
            {
                tweetBody = tweetBody.Replace("%artist%", "");
            }
            if (tweetBody.Contains("%album%"))
            {
                tweetBody = tweetBody.Replace("%album%", album);
            }
            else
            {
                tweetBody = tweetBody.Replace("%album%", "");
            }
            
            return tweetBody;
        }

        public string GetOAuthUrl()
        {
            string url = null;
            lorelei.GetOAuthUrl(out url);
            return url;
        }

        public void GetAccessToken(string pin, out string accessToken, out string accessTokenSecret)
        {
            lorelei.GetAccessToken(pin, out accessToken, out accessTokenSecret);
        }
    }
}
