// System
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// Internal
using VDownload.Core.Services;
using VDownload.Core.Enums.Media;



namespace VDownload.Core.Sources
{
    class Local
    {
        #region MAIN

        // GET VIDEOS FROM PLAYLIST (Dictionary<{id}, ({source_type}, {id}, {options})>
        public static async Task<Dictionary<int, (SourceType, string, Dictionary<string, string>)>> GetVideos(string path) 
        {
            // Read file
            string[] lines = await File.ReadAllLinesAsync(path);

            // Pack into dictionary
            Dictionary<int, (SourceType, string, Dictionary<string, string>)> videos = new();
            int index = 0;
            foreach (string l in lines)
            {
                // Arguments
                string[] args = l.Split(' ');
                var options = Args.Parse(args[1..]);
                if (options.ContainsKey("option_preset"))
                {
                    try
                    {
                        var optionsFromPreset = OptionPreset.GetOptions(options["option_preset"]);
                        var optionsNew = optionsFromPreset;
                        foreach (string k in options.Keys)
                        {
                            optionsNew[k] = options[k];
                        }
                        options = optionsNew;
                    }
                    catch { }
                }

                // Source switch
                string url = args[0];
                var source = Source.Get(url);
                switch (source.Item1)
                {
                    case (SourceType.YoutubeVideo): 
                    case (SourceType.TwitchVod):
                    case (SourceType.TwitchClip):
                    {
                        videos[index] = (source.Item1, url, options);
                        index++;
                        break;
                    }
                    case (SourceType.TwitchChannelP):
                    {
                        var playlistVideosT = await Twitch.GetChannelVideos(source.Item2);
                        options.Remove("stream");
                        options.Remove("vstream");
                        options.Remove("astream");
                        foreach (string u in playlistVideosT.Values)
                        {
                            videos[index] = (SourceType.TwitchVod, u, options);
                            index++;
                        }
                        break;
                    }
                    case (SourceType.LocalP):
                    {
                        var playlistVideosL = await GetVideos(source.Item2);
                        options.Remove("stream");
                        options.Remove("vstream");
                        options.Remove("astream");
                        foreach (var v in playlistVideosL.Values)
                        {
                            var localOptions = v.Item3;
                            foreach (var o in options)
                            {
                                localOptions[o.Key] = o.Value;
                            }
                            videos[index] = (v.Item1, v.Item2, localOptions);
                            index++;
                        }
                        break;
                    }
                }
            }

            // Return videos
            return videos;
        }

        #endregion
    }
}
