using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Services;
using VDownload.Core.Sources;

namespace VDownload.Main
{
    class MediaDownload
    {
        public static async Task TwitchVod(string id, Dictionary<string, string> options)
        {
            string uniqueID = VideoID.Get();
            string tempOutputFolderPath = $@"{Core.Globals.Path.Temp}\{uniqueID}\";
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
                Console.WriteLine();
                Console.WriteLine(Output.Get(@"media_download\_main.out",
                    args: new()
                    {
                        metadata["title"],
                        "Twitch",
                        metadata["id"],
                        $"Stream ID: {stream_id} ({streams[stream_id]["quality"]}p{streams[stream_id]["fps"]})"
                    }
                ));

                // Download stream
                string tempDownloadPath = $@"{tempOutputFolderPath}\stream\";
                Console.WriteLine();
                Console.Write(Output.Get(@"media_download\downloading.out"));
                Stopwatch downloadTime = new();
                downloadTime.Start();
                await Twitch.DownloadVodStream(id, stream_id, tempDownloadPath);
                downloadTime.Stop();
                Console.WriteLine(Output.Get(@"media_download\done_in.out",
                    new()
                    {
                        downloadTime.Elapsed.TotalSeconds.ToString()
                    }
                ));

                // Merge
                string tempMergePath = $@"{tempOutputFolderPath}\merged.ts";
                Console.Write(Output.Get(@"media_download\merging.out"));
                Stopwatch mergingTime = new();
                mergingTime.Start();
                using (FileStream mergedStream = new(tempMergePath, FileMode.Create, FileAccess.Write))
                {
                    foreach (string f in Directory.GetFiles(tempDownloadPath, "*.ts"))
                    {
                        if (File.Exists(f))
                        {
                            byte[] bytesPart = File.ReadAllBytes(f);
                            mergedStream.Write(bytesPart, 0, bytesPart.Length);

                            try
                            {
                                File.Delete(f);
                            }
                            catch { }
                        }
                    }
                }
                mergingTime.Stop();
                Console.WriteLine(Output.Get(@"media_download\done_in.out",
                    new()
                    {
                        mergingTime.Elapsed.TotalSeconds.ToString()
                    }
                ));

                // Convert
                string oldMergePath = tempMergePath;
                string tempConvertPath = $@"{tempOutputFolderPath}\converted.{extension}";
                if (onlyaudio & !onlyvideo)
                {
                    Console.Write(Output.Get(@"media_download\converting.out"));
                    Stopwatch convertTime = new();
                    convertTime.Start();
                    await FFmpeg.OnlyAudio(tempMergePath, tempConvertPath);
                    convertTime.Stop();
                    Console.WriteLine(Output.Get(@"media_download\done_in.out",
                        new()
                        {
                            convertTime.Elapsed.TotalSeconds.ToString()
                        }
                    ));
                    try
                    {
                        File.Delete(oldMergePath);
                    }
                    catch { }
                }
                else if (onlyvideo & !onlyaudio)
                {
                    Console.Write(Output.Get(@"media_download\converting.out"));
                    Stopwatch convertTime = new();
                    convertTime.Start();
                    await FFmpeg.OnlyVideo(tempMergePath, tempConvertPath);
                    convertTime.Stop();
                    Console.WriteLine(Output.Get(@"media_download\done_in.out",
                        new()
                        {
                            convertTime.Elapsed.TotalSeconds.ToString()
                        }
                    ));
                    try
                    {
                        File.Delete(oldMergePath);
                    }
                    catch { }
                }
                else
                {
                    if (Path.GetExtension(tempMergePath).Trim('.') != extension)
                    {
                        Console.Write(Output.Get(@"media_download\converting.out"));
                        Stopwatch convertTime = new();
                        convertTime.Start();
                        await FFmpeg.Convert(tempMergePath, tempConvertPath);
                        convertTime.Stop();
                        Console.WriteLine(Output.Get(@"media_download\done_in.out",
                            new()
                            {
                                convertTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                        try
                        {
                            File.Delete(oldMergePath);
                        }
                        catch { }
                    }
                    else
                    {
                        tempConvertPath = tempMergePath;
                    }
                }

                // Trim start
                string oldConvertPath = tempConvertPath;
                string tempTrimPath = $@"{tempOutputFolderPath}\trimmed.{extension}";
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
                    if ((startSec != 0 || endSec != baseVideoDuration.TotalSeconds) & startSec < endSec)
                    {
                        Console.Write(Output.Get(@"media_download\trimming.out"));
                        Stopwatch trimTime = new();
                        trimTime.Start();
                        await FFmpeg.Trim(tempConvertPath, tempTrimPath, startSec, endSec);
                        trimTime.Stop();
                        Console.WriteLine(Output.Get(@"media_download\done_in.out",
                            new()
                            {
                                trimTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                        try
                        {
                            File.Delete(oldConvertPath);
                        }
                        catch { }
                    }
                    else
                    {
                        tempTrimPath = tempConvertPath;
                    }
                }
                else
                {
                    tempTrimPath = tempConvertPath;
                }

                // Move to output destination
                string outPath = $@"{output_path}\{filename}.{extension}";
                File.Move(tempTrimPath, outPath, true);

                // Free id
                Directory.Delete(tempOutputFolderPath, true);
                VideoID.Free(uniqueID);

                // Done
                Console.WriteLine();
                Console.WriteLine(Output.Get(@"media_download\done.out"));
            }
            catch (Exception e)
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

        public static async Task TwitchClip(string id, Dictionary<string, string> options)
        {
            string uniqueID = VideoID.Get();
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
                Console.WriteLine();
                Console.WriteLine(Output.Get(@"media_download\_main.out",
                    args: new()
                    {
                        metadata["title"],
                        "Twitch",
                        metadata["id"],
                        $"Stream ID: {stream_id} ({streams[stream_id]["quality"]}p{streams[stream_id]["fps"]})"
                    }
                ));

                // Download stream
                string tempDownloadPath = $@"{tempOutputFolderPath}\stream{Path.GetExtension(streams[stream_id]["url"])}";
                Console.WriteLine();
                Console.Write(Output.Get(@"media_download\downloading.out"));
                Stopwatch downloadTime = new();
                downloadTime.Start();
                await Twitch.DownloadClipStream(id, stream_id, tempDownloadPath);
                downloadTime.Stop();
                Console.WriteLine(Output.Get(@"media_download\done_in.out", 
                    new() 
                    { 
                        downloadTime.Elapsed.TotalSeconds.ToString() 
                    }
                ));

                // Convert
                string tempConvertPath = $@"{tempOutputFolderPath}\converted.{extension}";
                if (onlyaudio & !onlyvideo)
                {
                    Console.Write(Output.Get(@"media_download\converting.out"));
                    Stopwatch convertTime = new();
                    convertTime.Start();
                    await FFmpeg.OnlyAudio(tempDownloadPath, tempConvertPath);
                    convertTime.Stop();
                    Console.WriteLine(Output.Get(@"media_download\done_in.out",
                        new()
                        {
                            convertTime.Elapsed.TotalSeconds.ToString()
                        }
                    ));
                }
                else if (onlyvideo & !onlyaudio)
                {
                    Console.Write(Output.Get(@"media_download\converting.out"));
                    Stopwatch convertTime = new();
                    convertTime.Start();
                    await FFmpeg.OnlyVideo(tempDownloadPath, tempConvertPath);
                    convertTime.Stop();
                    Console.WriteLine(Output.Get(@"media_download\done_in.out",
                        new()
                        {
                            convertTime.Elapsed.TotalSeconds.ToString()
                        }
                    ));
                }
                else
                {
                    if (Path.GetExtension(tempDownloadPath).Trim('.') != extension)
                    {
                        Console.Write(Output.Get(@"media_download\converting.out"));
                        Stopwatch convertTime = new();
                        convertTime.Start();
                        await FFmpeg.Convert(tempDownloadPath, tempConvertPath);
                        convertTime.Stop();
                        Console.WriteLine(Output.Get(@"media_download\done_in.out",
                            new()
                            {
                                convertTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                    }
                    else
                    {
                        tempConvertPath = tempDownloadPath;
                    }
                }

                // Trim start
                string tempTrimPath = $@"{tempOutputFolderPath}\trimmed.{extension}";
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
                    if ((startSec != 0 || endSec != baseVideoDuration.TotalSeconds) & startSec < endSec)
                    {
                        Console.Write(Output.Get(@"media_download\trimming.out"));
                        Stopwatch trimTime = new();
                        trimTime.Start();
                        await FFmpeg.Trim(tempConvertPath, tempTrimPath, startSec, endSec);
                        trimTime.Stop();
                        Console.WriteLine(Output.Get(@"media_download\done_in.out",
                            new()
                            {
                                trimTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                    }
                    else
                    {
                        tempTrimPath = tempConvertPath;
                    }
                }
                else
                {
                    tempTrimPath = tempConvertPath;
                }

                // Move to output destination
                string outPath = $@"{output_path}\{filename}.{extension}";
                File.Copy(tempTrimPath, outPath, true);

                // Free id
                Directory.Delete(tempOutputFolderPath, true);
                VideoID.Free(uniqueID);

                // Done
                Console.WriteLine();
                Console.WriteLine(Output.Get(@"media_download\done.out"));
            }
            catch (Exception e)
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
}
