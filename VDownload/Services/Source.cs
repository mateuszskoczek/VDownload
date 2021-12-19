// Internal
using VDownload.Objects.Enums;

// System
using System;
using System.Linq;

namespace VDownload.Services
{
    internal class Source
    {
        public static (VideoSource, string) GetVideoSourceData(Uri url)
        {
            // Twitch VOD
            if (url.Host == "www.twitch.tv" && url.Segments.Contains("videos/"))
            {
                return (VideoSource.TwitchVod, url.Segments[url.Segments.Length - 1].Replace("/", ""));
            }

            // Twitch Clip
            else if ((url.Host == "www.twitch.tv" && url.Segments.Contains("clip/")) || url.Host == "clips.twitch.tv")
            {
                return (VideoSource.TwitchClip, url.Segments[url.Segments.Length - 1].Replace("/", ""));
            }

            // Youtube Video
            else if (url.Host == "www.youtube.com" && url.Segments.Contains("watch"))
            {
                return (VideoSource.YoutubeVideo, url.Query.Replace("?", "").Split('&')[0].Replace("v=", ""));
            }
            else if (url.Host == "youtu.be")
            {
                return (VideoSource.YoutubeVideo, url.Segments[url.Segments.Length - 1]);
            }

            // Unknown
            else
            {
                return (VideoSource.Null, "");
            }
        }
    }
}
