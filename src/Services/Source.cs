using System.Collections.Generic;

namespace VDownload.Services
{
    class Source
    {
        #region CONSTANTS

        private static readonly Dictionary<string, List<string>> urlIndicators = new()
        {
            { "youtube_single", new(){ "youtube.com/watch?v=", "youtu.be" } },
            { "youtube_playlist", new(){ "youtube.com/playlist?list=" } },
            { "twitch_vod", new() { "twitch.tv/videos" } },
        }; // URL INDICATORS

        #endregion CONSTANTS





        #region MAIN

        // GET SOURCE WEBPAGE
        public static string Get(string url)
        {
            // Type
            foreach (string w in urlIndicators.Keys)
            {
                // Url indicator
                foreach (string s in urlIndicators[w])
                {
                    if (url.Contains(s))
                    {
                        return w;
                    }
                }
            }
            return null;
        }

        #endregion MAIN
    }
}
