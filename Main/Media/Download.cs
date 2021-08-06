// System
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// External
using Dasync.Collections;

// Internal
using VDownload.Core.Services;
using VDownload.Core.Sources;
using VDownload.Core.Enums.Media;
using VDownload.Core.Enums.FFmpeg;
using System.Net;

namespace VDownload.Main.Media
{
    class Download
    {
        #region CONSTANTS

        private static (int, int) CursorPosition;

        #endregion



        #region MAIN

        // DOWNLOAD VIDEO OR PLAYLIST
        public static async Task Switch(string[] args)
        {
            // Get videos
            List<List<string>> videos = new();
            List<string> video = new();
            foreach (string a in args[1..])
            {
                if (a[..2] != "--" && !a.Replace(";", "").All(char.IsDigit))
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
            videos.Add(video);
            videos.RemoveAt(0);

            // Filter videos
            List<(SourceType, string, Dictionary<string, string>)> validVideos = new();
            foreach (var v in videos)
            {
                // Options
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


                string url = v[0];
                var source = Source.Get(url);
                switch (source.Item1)
                {
                    case (SourceType.Null):
                    {
                        break;
                    }
                    case (SourceType.LocalP):
                    {
                        var playlistVideos = await Local.GetVideos(source.Item2);
                        foreach (var p in playlistVideos.Values)
                        {
                            var pOptions = p.Item3;
                            options.Remove("stream");
                            options.Remove("vstream");
                            options.Remove("astream");
                            foreach (var o in options)
                            {
                                pOptions[o.Key] = o.Value;
                            }
                            validVideos.Add((p.Item1, p.Item2, pOptions));
                        }
                        break;
                    }
                    case (SourceType.TwitchChannelP):
                    {
                        var playlistVideos = await Twitch.GetChannelVideos(source.Item2);
                        foreach (var p in playlistVideos.Values)
                        {
                            options.Remove("stream");
                            options.Remove("vstream");
                            options.Remove("astream");
                            validVideos.Add((SourceType.TwitchVod, p, options));
                        }
                        break;
                    }
                    default:
                    {
                        validVideos.Add((source.Item1, source.Item2, options));
                        break;
                    }
                }
            }

            // Redirect to downloading
            if (validVideos.Count == 0)
            {
                Main.Info.Help(args);
            }
            else if (validVideos.Count == 1) 
            {
                var v = validVideos[0];
                switch (v.Item1)
                {
                    case SourceType.YoutubeVideo: await YoutubeVideo(v.Item2, v.Item3); break;
                    case SourceType.TwitchVod: await TwitchVod(v.Item2, v.Item3); break;
                    case SourceType.TwitchClip: await TwitchClip(v.Item2, v.Item3); break;
                }
            }
            else
            {
                // Select videos to download
                IEnumerable<int> videoIds;
                if (args[^1].Replace(";", "").All(char.IsDigit))
                {
                    videoIds = from id in args[^1].Split(';') select int.Parse(id);
                }
                else
                {
                    videoIds = Enumerable.Range(0, validVideos.Count);
                }

                // Start downloading message
                Console.WriteLine();
                Console.WriteLine(Output.Get(@"media\download\playlist\_main.out", new() { videoIds.Count().ToString() }));
                Console.WriteLine();

                // Download videos
                await videoIds.ParallelForEachAsync(
                    async id =>
                    {
                        var video = validVideos[id];
                        SourceType type = video.Item1;
                        string video_id = video.Item2;
                        var options = video.Item3;
                        string title = "null";
                        string typeS = "null";
                        switch (type)
                        {
                            case (SourceType.YoutubeVideo): typeS = "yt"; break;
                            case (SourceType.TwitchVod): title = (await Twitch.GetVodMetadata(video_id))["title"]; typeS = "ttv"; break;
                            case (SourceType.TwitchClip): title = (await Twitch.GetClipMetadata(video_id))["title"]; typeS = "ttc"; break;
                        }

                        // Start message
                        string startMessage = Output.Get(@"media\download\playlist\start.out",
                            new()
                            {
                                "",
                                typeS,
                                video_id
                            }
                        );
                        int startMessageLength = startMessage.Length;
                        int titleLength = (Console.WindowWidth - startMessageLength) - 30;
                        title = StringC.Cut(title, titleLength);
                        startMessage = Output.Get(@"media\download\playlist\start.out",
                            new()
                            {
                                title,
                                typeS,
                                video_id
                            }
                        );
                        startMessageLength = startMessage.Length;

                        // Get start cursor position
                        int positionY = Console.GetCursorPosition().Top;
                        Console.WriteLine(startMessage);

                        // Start
                        string endMessage = "";
                        Stopwatch time = new();
                        time.Start();
                        try
                        {
                            switch (type)
                            {
                                case (SourceType.YoutubeVideo): await YoutubeVideo(video_id, options, true); break;
                                case (SourceType.TwitchVod): await TwitchVod(video_id, options, true); break;
                                case (SourceType.TwitchClip): await TwitchClip(video_id, options, true); break;
                            }
                            time.Stop();
                            endMessage = Output.Get(@"media\download\done_in.out",
                               new()
                               {
                                   time.Elapsed.Seconds.ToString()
                               }
                            );
                        }
                        catch (Exception e)
                        {
                            endMessage = Output.Get(@"media\download\playlist\_error.out", new() { e.GetType().Name });
                        }
                        finally
                        {
                            // Get end cursor position
                            int endPositionX = Console.GetCursorPosition().Left;
                            int endPositionY = Console.GetCursorPosition().Top;

                            // End output
                            Console.SetCursorPosition(startMessageLength, positionY);
                            Console.WriteLine(endMessage);
                            Console.SetCursorPosition(endPositionX, endPositionY);
                        }
                    },
                    maxDegreeOfParallelism: int.Parse(Config.Read("max_downloaded_videos"))
                );
                Console.WriteLine();
                Console.WriteLine(Output.Get(@"media\download\done.out"));
            }
        }

        #endregion



        #region SOURCES

        // YOUTUBE VIDEO
        private static async Task YoutubeVideo(string id, Dictionary<string, string> options, bool playlist = false)
        {
            // Get task ID
            string taskID = TaskID.Get();
            string tempOutputFolderPath = $@"{Core.Globals.Path.Temp}\{taskID}\";
            try
            {

            }
            catch (Exception e)
            {
                // Free id
                try
                {
                    Directory.Delete(tempOutputFolderPath, true);
                    TaskID.Free(taskID);
                }
                catch { }

                if (playlist)
                {
                    throw;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(Output.Get(@"_global\error.out",
                        new()
                        {
                            Output.Get(@"media_download\error.out"),
                            $"\n{e.Message}",
                        }
                    ));
                }
            }
        }


        // TWITCH VOD
        private static async Task TwitchVod(string id, Dictionary<string, string> options, bool playlist = false)
        {
            // Get task ID
            string taskID = TaskID.Get();
            string tempOutputFolderPath = $@"{Core.Globals.Path.Temp}\{taskID}\";
            try
            {
                // Get metadata
                var metadata = await Twitch.GetVodMetadata(id);

                // Options
                Dictionary<string, string> filenameCode = new()
                {
                    { "%title%", metadata["title"] },
                    { "%author%", metadata["author_name"] },
                    { "%duration%", metadata["duration"] },
                    { "%views%", metadata["views"] },
                    { "%pub_date%", metadata["date"] },
                    { "%act_date%", DateTime.Now.ToString(Config.Read("date_format")) },
                    { "%id%", metadata["id"] },
                };
                bool onlyaudio = options.ContainsKey("onlyaudio");
                bool onlyvideo = options.ContainsKey("onlyvideo");
                string filename = Config.Read("default_output_filename");
                if (options.ContainsKey("filename") && !(options["filename"] == null))
                {
                    filename = options["filename"];
                }
                filename = Filename.Process(filename, filenameCode);
                string output_path = Config.Read("default_output_path");
                if (options.ContainsKey("output_path") && !(options["output_path"] == null))
                {
                    output_path = options["output_path"];
                }
                string extension;
                if (onlyaudio && !onlyvideo)
                {
                    extension = Config.Read("default_output_aext");
                    if (options.ContainsKey("audio_ext") && !(options["audio_ext"] == null))
                    {
                        extension = options["audio_ext"];
                    }
                }
                else
                {
                    extension = Config.Read("default_output_vext");
                    if (options.ContainsKey("video_ext") && !(options["video_ext"] == null))
                    {
                        extension = options["video_ext"];
                    }
                }

                // Get streams
                var streams = await Twitch.GetVodStreams(id);
                if (options.ContainsKey("stream") && options["stream"] != null && int.TryParse(options["stream"], out int stream_id)) { } // Select stream
                else { stream_id = Twitch.SelectBestStream(streams); }

                // Start downloading
                Directory.CreateDirectory(tempOutputFolderPath);
                if (!playlist) Console.WriteLine();
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\single\_main.out",
                    args: new()
                    {
                        metadata["title"],
                        "Twitch",
                        metadata["id"],
                        $"Stream ID: {stream_id} ({streams[stream_id]["quality"]}p{Math.Ceiling(double.Parse(streams[stream_id]["fps"].Replace('.', ',')))})"
                    }
                ));

                // Download stream
                string tempDownloadPath = $@"{tempOutputFolderPath}\stream.ts";
                if (!playlist) Console.WriteLine();
                if (!playlist) Console.Write(Output.Get(@"media\download\single\downloading.out"));
                CursorPosition = Console.GetCursorPosition();
                Console.WriteLine("(0,00%)");
                Stopwatch downloadTime = new();
                downloadTime.Start();
                string[] chunks = await Twitch.GetVodStreamChunks(streams[stream_id]["url"]);
                using (FileStream mergedStream = new(tempDownloadPath, FileMode.Create, FileAccess.Write))
                {
                    int fileCount = 0;
                    Task<byte[]> chunkTask = Twitch.DownloadVodChunk(chunks[0]);
                    byte[] chunk = await chunkTask;
                    byte[] chunkCopy = chunk;
                    Task writeTask = mergedStream.WriteAsync(chunkCopy, 0, chunk.Length);
                    foreach (string url in chunks[1..])
                    {
                        chunkTask = Twitch.DownloadVodChunk(url);
                        chunk = await chunkTask;
                        chunkCopy = chunk;
                        await writeTask;
                        fileCount++;
                        Console.SetCursorPosition(CursorPosition.Item1, CursorPosition.Item2);
                        Console.WriteLine(((double)fileCount / (double)chunks.Length).ToString("(0.00%)"));
                        writeTask = mergedStream.WriteAsync(chunkCopy, 0, chunk.Length);
                    }
                }
                downloadTime.Stop();
                Console.SetCursorPosition(CursorPosition.Item1, CursorPosition.Item2);
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\done_in.out",
                   new()
                   {
                       Math.Ceiling(downloadTime.Elapsed.TotalSeconds).ToString()
                   }
                ));
                Environment.Exit(0);

                // FFmpeg init
                var FFmpegTask = new FFmpeg();
                FFmpegTask.Input(tempDownloadPath);
                FFmpegTask.Speed(Speed.Medium);
                FFmpegTask.AvoidNegativeTS(AvoidNegativeTS.MakeZero);

                // Channel disabling
                if (onlyvideo & !onlyaudio)
                {
                    FFmpegTask.DisableChannel(Channel.Audio);
                    FFmpegTask.CopyChannel(Channel.Video);
                }
                else if (onlyaudio & !onlyvideo)
                {
                    FFmpegTask.DisableChannel(Channel.Video);
                }

                // Trim
                if ((options.ContainsKey("trim_start") && options["trim_start"] != null) || (options.ContainsKey("trim_end") && options["trim_end"] != null))
                {
                    var baseVideoDuration = TimeSpan.Parse(metadata["duration"]);
                    double startSec = 0;
                    if (options.ContainsKey("trim_start") && TimeSpan.TryParse(options["trim_start"], out TimeSpan start) && start.TotalSeconds > 0 && start.TotalSeconds < baseVideoDuration.TotalSeconds)
                    {
                        startSec = start.TotalSeconds;
                    }
                    double endSec = baseVideoDuration.TotalSeconds;
                    if (options.ContainsKey("trim_end") && TimeSpan.TryParse(options["trim_end"], out TimeSpan end) && end.TotalSeconds > 0 && end.TotalSeconds < baseVideoDuration.TotalSeconds)
                    {
                        endSec = end.TotalSeconds;
                    }
                    if (startSec < endSec)
                    {
                        if (startSec != 0)
                        {
                            FFmpegTask.TrimStart(startSec);
                        }
                        if (endSec != baseVideoDuration.TotalSeconds)
                        {
                            FFmpegTask.TrimEnd(endSec);
                        }
                    }
                }

                // Start FFmpeg processing
                string tempOutputPath = $@"{tempOutputFolderPath}\processed.{extension}";
                FFmpegTask.Output(tempOutputPath);
                if (!playlist) Console.Write(Output.Get(@"media\download\single\processing.out"));
                Stopwatch processTime = new();
                processTime.Start();
                await FFmpegTask.StartAsync();
                processTime.Stop();
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\done_in.out",
                    new()
                    {
                        Math.Ceiling(processTime.Elapsed.TotalSeconds).ToString()
                    }
                ));

                // Move to output destination
                string outPath = $@"{output_path}\{filename}.{extension}";
                File.Move(tempOutputPath, outPath, true);

                // Free id
                Directory.Delete(tempOutputFolderPath, true);
                TaskID.Free(taskID);

                // Done
                if (!playlist) Console.WriteLine();
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\done.out"));
            }
            catch (Exception e)
            {
                // Free id
                try
                {
                    Directory.Delete(tempOutputFolderPath, true);
                    TaskID.Free(taskID);
                }
                catch { }

                if (playlist)
                {
                    throw;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(Output.Get(@"_global\error.out",
                        new()
                        {
                            Output.Get(@"media\download\single\_error.out"),
                            $"\n{e.Message}",
                        }
                    ));
                }
            }
        }


        // TWITCH CLIP
        private static async Task TwitchClip(string id, Dictionary<string, string> options, bool playlist = false)
        {
            string uniqueID = TaskID.Get();
            string tempOutputFolderPath = $@"{Core.Globals.Path.Temp}\{uniqueID}\";
            try
            {
                // Get metadata
                var metadata = await Twitch.GetClipMetadata(id);

                // Options
                Dictionary<string, string> filenameCode = new()
                {
                    { "%title%", metadata["title"] },
                    { "%author%", metadata["author_name"] },
                    { "%broadcaster%", metadata["broadcaster_name"] },
                    { "%duration%", metadata["duration"] },
                    { "%views%", metadata["views"] },
                    { "%pub_date%", metadata["date"] },
                    { "%act_date%", DateTime.Now.ToString(Config.Read("date_format")) },
                    { "%id%", metadata["id"] },
                };
                bool onlyaudio = options.ContainsKey("onlyaudio");
                bool onlyvideo = options.ContainsKey("onlyvideo");
                string filename = Config.Read("default_output_filename");
                if (options.ContainsKey("filename") && !(options["filename"] == null))
                {
                    filename = options["filename"];
                }
                filename = Filename.Process(filename, filenameCode);
                string output_path = Config.Read("default_output_path");
                if (options.ContainsKey("output_path") && !(options["output_path"] == null))
                {
                    output_path = options["output_path"];
                }
                string extension;
                if (onlyaudio && !onlyvideo)
                {
                    extension = Config.Read("default_output_aext");
                    if (options.ContainsKey("audio_ext") && !(options["audio_ext"] == null))
                    {
                        extension = options["audio_ext"];
                    }
                }
                else
                {
                    extension = Config.Read("default_output_vext");
                    if (options.ContainsKey("video_ext") && !(options["video_ext"] == null))
                    {
                        extension = options["video_ext"];
                    }
                }

                // Get streams
                var streams = await Twitch.GetClipStreams(id);
                if (options.ContainsKey("stream") && options["stream"] != null && int.TryParse(options["stream"], out int stream_id)) { } // Select stream
                else { stream_id = Twitch.SelectBestStream(streams); }

                // Start downloading
                Directory.CreateDirectory(tempOutputFolderPath);
                if (!playlist) Console.WriteLine();
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\single\_main.out",
                    args: new()
                    {
                        metadata["title"],
                        "Twitch",
                        metadata["id"],
                        $"Stream ID: {stream_id} ({streams[stream_id]["quality"]}p{Math.Ceiling(double.Parse(streams[stream_id]["fps"].Replace('.', ',')))})"
                    }
                ));

                // Download stream
                string tempDownloadPath = $@"{tempOutputFolderPath}\stream{Path.GetExtension(streams[stream_id]["url"])}";
                if (!playlist) Console.WriteLine();
                if (!playlist) Console.Write(Output.Get(@"media\download\single\downloading.out"));
                CursorPosition = Console.GetCursorPosition();
                Console.WriteLine("(0,00%)");
                Stopwatch downloadTime = new();
                downloadTime.Start();
                await Twitch.DownloadClipStream(id, stream_id, tempDownloadPath, new DownloadProgressChangedEventHandler(DownloadProgressCallback));
                downloadTime.Stop();
                Console.SetCursorPosition(CursorPosition.Item1, CursorPosition.Item2);
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\done_in.out",
                    new()
                    {
                        Math.Ceiling(downloadTime.Elapsed.TotalSeconds).ToString()
                    }
                ));

                // FFmpeg init
                var FFmpegTask = new FFmpeg();
                FFmpegTask.Input(tempDownloadPath);
                FFmpegTask.Speed(Speed.Medium);

                // Channel disabling
                if (onlyvideo & !onlyaudio)
                {
                    FFmpegTask.DisableChannel(Channel.Audio);
                    FFmpegTask.CopyChannel(Channel.Video);
                }
                else if (onlyaudio & !onlyvideo)
                {
                    FFmpegTask.DisableChannel(Channel.Video);
                }

                // Trim
                if ((options.ContainsKey("trim_start") && options["trim_start"] != null) || (options.ContainsKey("trim_end") && options["trim_end"] != null))
                {
                    var baseVideoDuration = TimeSpan.Parse(metadata["duration"]);
                    double startSec = 0;
                    if (options.ContainsKey("trim_start") && TimeSpan.TryParse(options["trim_start"], out TimeSpan start) && start.TotalSeconds > 0 && start.TotalSeconds < baseVideoDuration.TotalSeconds)
                    {
                        startSec = start.TotalSeconds;
                    }
                    double endSec = baseVideoDuration.TotalSeconds;
                    if (options.ContainsKey("trim_end") && TimeSpan.TryParse(options["trim_end"], out TimeSpan end) && end.TotalSeconds > 0 && end.TotalSeconds < baseVideoDuration.TotalSeconds)
                    {
                        endSec = end.TotalSeconds;
                    }
                    if (startSec < endSec)
                    {
                        if (startSec != 0)
                        {
                            FFmpegTask.TrimStart(startSec);
                        }
                        if (endSec != baseVideoDuration.TotalSeconds)
                        {
                            FFmpegTask.TrimEnd(endSec);
                        }
                    }
                }

                // Start FFmpeg processing
                string tempOutputPath = $@"{tempOutputFolderPath}\processed.{extension}";
                FFmpegTask.Output(tempOutputPath);
                if (!playlist) Console.Write(Output.Get(@"media\download\single\processing.out"));
                Stopwatch processTime = new();
                processTime.Start();
                await FFmpegTask.StartAsync();
                processTime.Stop();
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\done_in.out",
                    new()
                    {
                        Math.Ceiling(processTime.Elapsed.TotalSeconds).ToString()
                    }
                ));

                // Move to output destination
                string outPath = $@"{output_path}\{filename}.{extension}";
                File.Move(tempOutputPath, outPath, true);

                // Free id
                Directory.Delete(tempOutputFolderPath, true);
                TaskID.Free(uniqueID);

                // Done
                if (!playlist) Console.WriteLine();
                if (!playlist) Console.WriteLine(Output.Get(@"media\download\done.out"));
            }
            catch (Exception e)
            {
                // Free id
                try
                {
                    Directory.Delete(tempOutputFolderPath, true);
                    TaskID.Free(uniqueID);
                }
                catch { }

                if (playlist)
                {
                    throw;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(Output.Get(@"_global\error.out",
                        new()
                        {
                            Output.Get(@"media\download\_error.out"),
                            $"\n{e.Message}",
                        }
                    ));
                }
            }

        }

        #endregion



        #region PROGRESS_CALLBACKS

        // FILE DOWNLOAD
        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.SetCursorPosition(CursorPosition.Item1, CursorPosition.Item2);
            Console.WriteLine(e.ProgressPercentage);
        }

        #endregion
    }
}
