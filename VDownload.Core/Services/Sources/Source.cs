using System.Text.RegularExpressions;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;

namespace VDownload.Core.Services.Sources
{
    public static class Source
    {
        #region CONSTANTS

        // VIDEO SOURCES REGULAR EXPRESSIONS
        private static readonly (Regex Regex, VideoSource Type)[] VideoSources = new (Regex Regex, VideoSource Type)[]
        {
            (new Regex(@"^https://www.twitch.tv/videos/(?<id>\d+)"), VideoSource.TwitchVod),
            (new Regex(@"^https://www.twitch.tv/\S+/clip/(?<id>[^?]+)"), VideoSource.TwitchClip),
            (new Regex(@"^https://clips.twitch.tv/(?<id>[^?]+)"), VideoSource.TwitchClip),
        };

        // PLAYLIST SOURCES REGULAR EXPRESSIONS
        private static readonly (Regex Regex, PlaylistSource Type)[] PlaylistSources = new (Regex Regex, PlaylistSource Type)[]
        {
            (new Regex(@"^https://www.twitch.tv/(?<id>[^?/]+)"), PlaylistSource.TwitchChannel),
        };

        #endregion



        #region METHODS

        // GET VIDEO SOURCE
        public static IVideo GetVideo(string url)
        {
            VideoSource source = VideoSource.Null;
            string id = string.Empty;
            foreach ((Regex Regex, VideoSource Type) Source in VideoSources)
            {
                Match sourceMatch = Source.Regex.Match(url);
                if (sourceMatch.Success)
                {
                    source = Source.Type;
                    id = sourceMatch.Groups["id"].Value;
                }
            }
            return GetVideo(source, id);
        }
        public static IVideo GetVideo(VideoSource source, string id)
        {
            IVideo videoService = null;
            switch (source)
            {
                case VideoSource.TwitchVod: videoService = new Twitch.Vod(id); break;
                case VideoSource.TwitchClip: videoService = new Twitch.Clip(id); break;
            }
            return videoService;
        }

        // GET PLAYLIST SOURCE
        public static IPlaylist GetPlaylist(string url)
        {
            PlaylistSource source = PlaylistSource.Null;
            string id = string.Empty;
            foreach ((Regex Regex, PlaylistSource Type) Source in PlaylistSources)
            {
                Match sourceMatch = Source.Regex.Match(url);
                if (sourceMatch.Success)
                {
                    source = Source.Type;
                    id = sourceMatch.Groups["id"].Value;
                }
            }
            return GetPlaylist(source, id);
        }
        public static IPlaylist GetPlaylist(PlaylistSource source, string id)
        {
            IPlaylist playlistService = null;
            switch (source)
            {
                case PlaylistSource.TwitchChannel: playlistService = new Twitch.Channel(id); break;
            }
            return playlistService;
        }

        #endregion
    }
}
