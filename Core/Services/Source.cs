// System
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace VDownload.Core.Services
{
    class Source
    {
        #region MAIN

        /*
         *  yt: - Youtube Video 
         *  ttv: - Twitch VOD
         *  ttc: - Twitch Clip
         *  pl-ttv: - Playlist: Twitch Channel
         */

        // GET SOURCE WEBPAGE
        public static (string, string) Get(string urlID)
        {
            // Youtube video
            if (urlID.Contains("yt:") && urlID.Replace("yt:", "").All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("yt:", "");
                return ("youtube_video", id);
            }
            else if (urlID.Contains("youtube.com/watch?v=") && urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtube.comwatch?v=", "").Split('&')[0].All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtube.comwatch?v=", "").Split('&')[0];
                return ("youtube_video", id);
            }
            else if (urlID.Contains("youtu.be") && urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtu.be", "").Split('?')[0].All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("youtu.be", "").Split('?')[0];
                return ("youtube_video", id);
            }

            // Twitch vod
            else if (urlID.Contains("ttv:") && urlID.Replace("ttv:", "").All(char.IsDigit))
            {
                string id = urlID.Replace("ttv:", "");
                return ("twitch_vod", id);
            }
            else if (urlID.Contains("twitch.tv/videos") && urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("twitch.tvvideos", "").Split('?')[0].All(char.IsDigit))
            {
                string id = urlID.Replace("/", "").Replace(@"\", "").Replace("http:", "").Replace("https:", "").Replace("www.", "").Replace("twitch.tvvideos", "").Split('?')[0];
                return ("twitch_vod", id);
            }

            // Twitch clip
            else if (urlID.Contains("ttc:") && urlID.Replace("ttc:", "").Split('-').Length == 2 && urlID.Replace("ttc:", "").Split('-')[0].All(char.IsLetter) && urlID.Replace("ttc:", "").Split('-')[1].All(char.IsLetterOrDigit))
            {
                string id = urlID.Replace("ttc:", "");
                return ("twitch_clip", id);
            }
            else if (urlID.Contains("twitch.tv") && urlID.Split('/')[^1].Split('-').Length == 2 && urlID.Split('/')[^1].Split('-')[0].All(char.IsLetter) && urlID.Split('/')[^1].Split('-')[1].All(char.IsLetterOrDigit))
            {
                string id = urlID.Split('/')[^1];
                return ("twitch_clip", id);
            }

            // Twitch channel
            else if (urlID.Contains("pl-ttv:") && !urlID.Replace("ttv:", "").Contains(@"\") && !urlID.Replace("ttv:", "").Contains(@"/"))
            {
                string id = urlID.Replace("pl-ttv:", "");
                return ("pl-twitch_channel", id);
            }
            else if (urlID.Contains("twitch.tv"))
            {
                string id = urlID.Replace(@"http:\\", "").Replace("http://", "").Replace(@"https:\\", "").Replace(@"https://", "").Replace("www.", "").Replace("twitch.tv", "").Replace(@"\", "").Replace("/", "").Split("?")[0];
                return ("pl-twitch_channel", id);
            }

            // Unknown
            else
            {
                return (null, null);
            }
        }

        #endregion
    }
}
