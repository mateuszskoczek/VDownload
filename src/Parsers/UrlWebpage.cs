using System.Collections.Generic;

namespace VDownload.Parsers
{
    class UrlWebpage
    {
        // CONSTANTS
        private static readonly Dictionary<string, List<string>> urlIndicators = new()
        {
            { "youtube_single", new(){ "youtube.com/watch?v=", "youtu.be" } },
            { "youtube_playlist", new(){ "youtube.com/playlist?list=" } }
        };


        // MAIN
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

    }
}
