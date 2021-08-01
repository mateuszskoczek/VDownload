// System
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

// External
using ConsoleTableExt;

// Internal
using VDownload.Core.Services;
using VDownload.Core.Sources;
using VDownload.Core.Enums.Media;



namespace VDownload.Main.Media
{
    class Info
    {
        #region MAIN

        // INFORMATIONS ABOUT VIDEO OR PLAYLIST
        public static async Task Switch(string[] args)
        {
            string output = "";
            try
            {
                string url = args[1];
                var source = Source.Get(url);
                switch (source.Item1)
                {
                    case (SourceType.YoutubeVideo):
                    {
                        // WIP
                        break;
                    }
                    case (SourceType.TwitchVod):
                    {
                        output = await TwitchVod(source.Item2);
                        break;
                    }
                    case (SourceType.TwitchClip):
                    {
                        output = await TwitchClip(source.Item2);
                        break;
                    }
                    case (SourceType.TwitchChannelP):
                    {
                        output = await PlaylistTwitchChannel(source.Item2);
                        break;
                    }
                    case (SourceType.LocalP):
                    {
                        output = await PlaylistLocal(source.Item2);
                        break;
                    }
                    default:
                    {
                        output = Output.Get(@"_global\error.out",
                            new()
                            {
                                Output.Get(@"media\list\_error\_main.out"),
                                Output.Get(@"media\list\_error\wrong_path_url_id.out"),
                            }
                        );
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"media\info\_error\_main.out"),
                        $"\n{e.Message}",
                    }
                );
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine(output);
            }
        }

        #endregion



        #region SOURCES

        // YOUTUBE VIDEO
        private static async Task<string> YoutubeVideo(string id)
        {
            return null;
        }


        // TWITCH VOD
        private static async Task<string> TwitchVod(string id)
        {
            // Get metadata
            var metadataTask = Twitch.GetVodMetadata(id);
            
            // Get streams
            var streams = await Twitch.GetVodStreams(id);

            // Build streams table
            List<List<object>> streamsTableData = new();
            foreach (var s in streams)
            {
                streamsTableData.Add(new()
                {
                    s.Key,
                    s.Value["quality"],
                    s.Value["fps"],
                    s.Value["video_codec"],
                    s.Value["audio_codec"]
                });
            }
            string streamsTable = ConsoleTableBuilder
                .From(streamsTableData)
                .WithColumn("ID", "Quality", "FPS", "Video codec", "Audio codec")
                .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Core.Globals.Table.Streams)
                .Export().ToString().Trim();
            
            // Wait for metadataTask
            var metadata = await metadataTask;

            // Output
            return Output.Get(@"media\info\twitch_vod\_main.out",
                new()
                {
                    metadata["title"],
                    metadata["id"],
                    metadata["author_name"],
                    metadata["date"],
                    metadata["duration"],
                    metadata["views"],
                    metadata["thumbnail"],
                    streamsTable
                }
            );
        }


        // TWITCH CLIP
        private static async Task<string> TwitchClip(string id)
        {
            // Get metadata
            var metadataTask = Twitch.GetClipMetadata(id);

            // Get streams
            var streams = await Twitch.GetClipStreams(id);

            // Build streams table
            List<List<object>> streamsTableData = new();
            foreach (var s in streams)
            {
                streamsTableData.Add(new()
                {
                    s.Key,
                    s.Value["quality"],
                    s.Value["fps"]
                });
            }
            string streamsTable = ConsoleTableBuilder
                .From(streamsTableData)
                .WithColumn("ID", "Quality", "FPS")
                .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Core.Globals.Table.Streams)
                .Export().ToString().Trim();

            // Wait for metadataTask
            var metadata = await metadataTask;

            // Output
            return Output.Get(@"media\info\twitch_clip\_main.out",
                new()
                {
                    metadata["title"],
                    metadata["id"],
                    metadata["author_name"],
                    metadata["broadcaster_name"],
                    metadata["date"],
                    metadata["duration"],
                    metadata["views"],
                    metadata["thumbnail"],
                    streamsTable
                }
            );
        }


        // PLAYLIST: LOCAL
        private static async Task<string> PlaylistLocal(string id)
        {
            // Get videos
            var videos = await Local.GetVideos(id);
            
            // Build videos list
            string videosList = "";
            if (videos.Count == 0)
            {
                videosList = Output.Get(@"media\info\playlist_no_videos.out");
            }
            else
            {
                foreach (var v in videos)
                {
                    Dictionary<string, string> metadata = new();
                    string type = "null";
                    switch (v.Value.Item1)
                    {
                        case (SourceType.YoutubeVideo): break;
                        case (SourceType.TwitchVod): metadata = await Twitch.GetVodMetadata(v.Value.Item2); type = "ttv"; break;
                        case (SourceType.TwitchClip): metadata = await Twitch.GetClipMetadata(v.Value.Item2); type = "ttc"; break;
                        default: continue;
                    }
                    int videoLength = $"{v.Key}: {metadata["author_name"]} - \"\" ({type}:{v.Value.Item2})".Length;
                    int titleLength = (Console.WindowWidth - videoLength) - 1;
                    videosList += $"{v.Key}: {metadata["author_name"]} - \"{StringC.Cut(metadata["title"], titleLength)}\" ({type}:{v.Value.Item2})\n";
                }
            }

            // Output
            return Output.Get(@"media\info\pl-local\_main.out",
                new()
                {
                    Path.GetFileName(id),
                    id,
                    videosList
                }
            );
        }


        // PLAYLIST: YOUTUBE PLAYLIST
        private static async Task<string> PlaylistYoutubePlaylist(string id)
        {
            return null;
        }


        // PLAYLIST: YOUTUBE CHANNEL
        private static async Task<string> PlaylistYoutubeChannel(string id)
        {
            return null;
        }


        // PLAYLIST: TWITCH CHANNEL
        private static async Task<string> PlaylistTwitchChannel(string id)
        {
            // Get metadata
            var metadataTask = Twitch.GetChannelMetadata(id);
            
            // Get videos
            var videos = await Twitch.GetChannelVideos(id);
            
            // Build videos list
            string videosList = "";
            if (videos.Count == 0)
            {
                videosList = Output.Get(@"media\info\playlist_no_videos.out");
            }
            else
            {
                foreach (var v in videos)
                {
                    var videoMetadata = await Twitch.GetVodMetadata(v.Value);
                    int videoLength = $"{v.Key}: {videoMetadata["author_name"]} - \"\" (ttv:{v.Value})".Length;
                    int titleLength = (Console.WindowWidth - videoLength) - 1;
                    videosList += $"{v.Key}: {videoMetadata["author_name"]} - \"{StringC.Cut(videoMetadata["title"], titleLength)}\" (ttv:{v.Value})\n";
                }
            }
            
            // Wait for metadataTask
            var metadata = await metadataTask;
            
            // Output
            return Output.Get(@"media\info\pl-twitch_channel\_main.out",
                new()
                {
                    metadata["name"],
                    metadata["id"],
                    videosList
                }
            );
        }

        #endregion
    }
}
