using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Core.Enums;

namespace VDownload.Core.Services
{
    public class Source
    {
        #region CONSTANTS

        private static readonly (Regex Regex, VideoSource Type)[] VideoSources = new (Regex Regex, VideoSource Type)[]
        {
            (new Regex(@"^https://www.twitch.tv/videos/(?<id>\d+)"), VideoSource.TwitchVod),
            (new Regex(@"^https://www.twitch.tv/\S+/clip/(?<id>[^?]+)"), VideoSource.TwitchClip),
            (new Regex(@"^https://clips.twitch.tv/(?<id>[^?]+)"), VideoSource.TwitchClip),
        };

        private static readonly (Regex Regex, PlaylistSource Type)[] PlaylistSources = new (Regex Regex, PlaylistSource Type)[]
        {
            (new Regex(@"^https://www.twitch.tv/(?<id>[^?]+)"), PlaylistSource.TwitchChannel),
        };

        #endregion



        #region METHODS

        public static (VideoSource Type, string ID) GetVideoSource(string url)
        {
            foreach ((Regex Regex, VideoSource Type) Source in VideoSources)
            {
                Match sourceMatch = Source.Regex.Match(url);
                if (sourceMatch.Success) return (Source.Type, sourceMatch.Groups["id"].Value);
            }
            return (VideoSource.Null, null);
        }

        public static (PlaylistSource Type, string ID) GetPlaylistSource(string url)
        {
            foreach ((Regex Regex, PlaylistSource Type) Source in PlaylistSources)
            {
                Match sourceMatch = Source.Regex.Match(url);
                if (sourceMatch.Success) return (Source.Type, sourceMatch.Groups["id"].Value);
            }
            return (PlaylistSource.Null, null);
        }

        #endregion
    }
}
