using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Services;

namespace VDownload.Main.Media
{
    class Download
    {
        #region MAIN

        // DOWNLOAD VIDEO OR PLAYLIST
        public static async Task Switch(string[] args)
        {
            List<List<string>> videos = new();
            List<string> video = new();
            foreach (string a in args)
            {
                if (a[..2] != "--")
                {
                    videos.Add(video);
                    video = new();
                    video.Add(a);
                }
                else
                {
                    video.Add(a);
                }
            }
            videos.RemoveAt(0);
            List<(string, string, Dictionary<string, string>)> validVideos = new();
            foreach (var v in videos)
            {
                string url = v[0];
                var options = Args.Parse(v.ToArray()[1..]);
                if (options.ContainsKey("option_preset"))
                {
                    try
                    {
                        var optionsFromPreset = Core.Services.OptionPreset.GetOptions(options["option_preset"]);
                        var optionsNew = optionsFromPreset;
                        foreach (string k in options.Keys)
                        {
                            optionsNew[k] = options[k];
                        }
                        options = optionsNew;
                    }
                    catch { }
                }
                var source = Source.Get(url);
                switch (source.Item1)
                {
                    case (null):
                        {
                            break;
                        }
                    case ("playlistlocal"):
                        {
                            //WIP
                            break;
                        }
                    default:
                        {
                            validVideos.Add((source.Item1, source.Item2, options));
                            break;
                        }
                }
            }
        }

        #endregion



        #region SOURCES_SINGLE



        #endregion



        #region SOURCES_PLAYLIST



        #endregion
    }
}
