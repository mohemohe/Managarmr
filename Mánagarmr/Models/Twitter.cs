﻿using System.Windows;
using Rhinemaidens;
using System;
using Mánagarmr.Helpers;

namespace Mánagarmr.Models
{
    internal class Twitter
    {
        private readonly Lorelei _lorelei = new Lorelei();

        public Twitter()
        {
            _lorelei.Initialize(consumerKey, consumerSecret);
        }

        public Twitter(string accessToken, string accessTokenSecret)
        {
            _lorelei.Initialize(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        private string consumerKey
        {
            get { return "WY93gCVpkgAMdVAVuu5yDYB0g"; }
        }

        private string consumerSecret
        {
            get { return "Q1GYjZd9F78NdbpCStl9u0lbMcqJGmJjqXnZS9NQqddMyqwS5t"; }
        }

        private string lastfmSearchUrl
        {
            get { return "http://www.last.fm/search?q="; }
        }

        private string youtubeSearchUrl
        {
            get { return "https://www.youtube.com/results?search_query="; }
        }

        public void Initialize(string accessToken, string accessTokenSecret)
        {
            _lorelei.Initialize(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        public void Tweet(string title, string artist, string album)
        {
            string tweetBody = GenerateTweetBody(title, artist, album);
            try
            {
                _lorelei.PostTweet(tweetBody);
            }
            catch (UnauthorizedException)
            {
                var mbp = new MessageBoxPack("Twitterの認証に失敗しました\n設定から再度認証してください", "ツイート失敗", MessageBoxImage.Error);
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
            catch (DuplicateTweetBodyException)
            {
                var mbp = new MessageBoxPack("同じ曲を連続してツイートできません", "ツイート失敗", MessageBoxImage.Error);
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
            catch (TwitterServerNotWorkingWellException)
            {
                var mbp = new MessageBoxPack("Twitterサーバーからの応答がおかしいです", "ツイート失敗", MessageBoxImage.Error);
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
            catch
            {
                var mbp = new MessageBoxPack("不明なエラーが発生しました", "ツイート失敗", MessageBoxImage.Error);
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
        }

        private string GenerateTweetBody(string title, string artist, string album)
        {
            string tweetBody = Settings.TweetTextFormat;

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

            if (Settings.TweetUrl == 1)
            {
                tweetBody += " " + lastfmSearchUrl + Uri.EscapeDataString(title) + "+" + Uri.EscapeDataString(artist);
            }
            else if (Settings.TweetUrl == 2)
            {
                tweetBody += " " + youtubeSearchUrl + Uri.EscapeDataString(title) + "+" + Uri.EscapeDataString(artist);
            }

            return tweetBody;
        }

        public string GetOAuthUrl()
        {
            string url;
            _lorelei.GetOAuthUrl(out url);
            return url;
        }

        public void GetAccessToken(string pin, out string accessToken, out string accessTokenSecret)
        {
            _lorelei.GetAccessToken(pin, out accessToken, out accessTokenSecret);
        }
    }
}