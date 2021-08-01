// System
using System.IO;
using System.Linq;

// Internal
using VDownload.Core.Enums.Media;



namespace VDownload.Core.Services
{
    class Source
    {
        #region MAIN

        /*
         *  yt: - Youtube Video 
         *  ttv: - Twitch VOD
         *  ttc: - Twitch Clip
         *  p-ytp - Playlist: Youtube Playlist
         *  p-ytc - Playlist: Youtube Channel
         *  p-ttv: - Playlist: Twitch Channel
         *  p-local = Playlist: Local Text File
         */

        // GET SOURCE WEBPAGE
        public static (SourceType, string) Get(string urlID)
        {
            // Playlist: Local Text File
            if (File.Exists(urlID.Replace("p-local:", "")))
            {
                string id = urlID.Replace("p-local:", "");
                return (SourceType.LocalP, id);
            }

            // Youtube video
            else if (urlID.Contains("yt:") && urlID.Replace("yt:", "").All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("yt:", "");
                return (SourceType.YoutubeVideo, id);
            }
            else if (urlID.Contains("youtube.com/watch?v=") && urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtube.comwatch?v=", "").Split('&')[0].All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtube.comwatch?v=", "").Split('&')[0];
                return (SourceType.YoutubeVideo, id);
            }
            else if (urlID.Contains("youtu.be") && urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtu.be", "").Split('?')[0].All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtu.be", "").Split('?')[0];
                return (SourceType.YoutubeVideo, id);
            }

            // Twitch vod
            else if (urlID.Contains("ttv:") && urlID.Replace("ttv:", "").All(char.IsDigit))
            {
                string id = urlID.Replace("ttv:", "");
                return (SourceType.TwitchVod, id);
            }
            else if (urlID.Contains("twitch.tv/videos") && urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("twitch.tvvideos", "").Split('?')[0].All(char.IsDigit))
            {
                string id = urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("twitch.tvvideos", "").Split('?')[0];
                return (SourceType.TwitchVod, id);
            }

            // Twitch clip
            else if (urlID.Contains("ttc:") && urlID.Replace("ttc:", "").Split('-').Length >= 2 && urlID.Replace("ttc:", "").Split('-')[0].All(char.IsLetter))
            {
                string id = urlID.Replace("ttc:", "");
                return (SourceType.TwitchClip, id);
            }
            else if (urlID.Contains("twitch.tv") && urlID.Split('/')[^1].Split('-').Length >= 2 && urlID.Split('/')[^1].Split('-')[0].All(char.IsLetter))
            {
                string id = urlID.Split('/')[^1];
                return (SourceType.TwitchClip, id);
            }

            // Twitch channel
            else if (urlID.Contains("p-ttv:") && !urlID.Replace("ttv:", "").Contains(@"\") && !urlID.Replace("ttv:", "").Contains(@"/"))
            {
                string id = urlID.Replace("pl-ttv:", "");
                return (SourceType.TwitchChannelP, id);
            }
            else if (urlID.Contains("twitch.tv"))
            {
                string id = urlID.Replace(@"http:\\", "").Replace("http://", "").Replace(@"https:\\", "").Replace(@"https://", "").Replace("www.", "").Replace("twitch.tv", "").Replace(@"\", "").Replace("/", "").Split("?")[0];
                return (SourceType.TwitchChannelP, id);
            }

            // Unknown
            else
            {
                return (SourceType.Null, null);
            }
        }

        #endregion
    }
}
