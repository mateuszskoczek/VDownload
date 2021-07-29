using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Services;
using VDownload.Core.Sources;
using ConsoleTableExt;

namespace VDownload.Main.Media
{
    class Info
    {
        #region MAIN

        // INFORMATIONS ABOUT VIDEO OR PLAYLIST
        public static async Task Switch(string[] args)
        {
            string url = args[1];
            var source = Source.Get(url);
            switch (source.Item1)
            {
                case ("youtube_video"):
                {
                    // WIP
                    break;
                }
                case ("twitch_vod"):
                {
                    await TwitchVod(source.Item2);
                    break;
                }
                case ("twitch_clip"):
                {
                    await TwitchClip(source.Item2);
                    break;
                }
                case ("pl-twitch_channel"):
                {
                    await PlaylistTwitchChannel(source.Item2);
                    break;
                }
                default:
                {
                    Console.WriteLine(Output.Get(@"_global\error.out",
                        new()
                        {
                            Output.Get(@"media\list\_error\_main.out"),
                            Output.Get(@"media\list\_error\wrong_path_url_id.out"),
                        }
                    ));
                    break;
                }
            }
        }

        #endregion



        #region SOURCES

        // YOUTUBE VIDEO
        private static async Task YoutubeVideo(string id)
        {

        }


        // TWITCH VOD
        private static async Task TwitchVod(string id)
        {
            string output = "";
            try
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
                output = Output.Get(@"media\info\twitch_vod\_main.out",
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


        // TWITCH CLIP
        private static async Task TwitchClip(string id)
        {
            string output = "";
            try
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
                output = Output.Get(@"media\info\twitch_clip\_main.out",
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


        // PLAYLIST: LOCAL
        private static async Task PlaylistLocal(string path)
        {

        }


        // PLAYLIST: YOUTUBE PLAYLIST
        private static async Task PlaylistYoutubePlaylist(string id)
        {

        }


        // PLAYLIST: YOUTUBE CHANNEL
        private static async Task PlaylistYoutubeChannel(string id)
        {

        }


        // PLAYLIST: TWITCH CHANNEL
        private static async Task PlaylistTwitchChannel(string id)
        {
            string output = "";
            try
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
                output = Output.Get(@"media\info\pl-twitch_channel\_main.out",
                    new()
                    {
                        metadata["name"],
                        metadata["id"],
                        videosList
                    }
                );
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
    }
}
