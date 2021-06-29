using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Playlists;
using YoutubeExplode.Exceptions;
using ConsoleTableExt;
using Dasync.Collections;
using VDownload.Services;

namespace VDownload.Sources
{
    class Youtube
    {
        # region CONSTANTS

        private static readonly YoutubeClient client = new(); // YOUTUBE CLIENT
        private static readonly int maxFilesDownloadingTasks = 3; // MAX NUMBER OF FILES DOWNLOADED ASYNCHRONOUSLY
        static Youtube()
        {
            try
            {
                int newMaxFilesDownloadingTasks = int.Parse(Config.Main.ReadKey("max_files_downloading_tasks"));
                if (newMaxFilesDownloadingTasks > 0)
                    maxFilesDownloadingTasks = newMaxFilesDownloadingTasks;
            }
            catch { }
        }

        #endregion CONSTANTS





        #region MAIN_INFO

        // WRITE INFORMATIONS ABOUT VIDEO (METADATA, VIDEO AND AUDIO STREAMS)
        public static async Task VideoInfo(string url)
        {
            string output; // Init output
            try
            {
                // Get video metadata
                var metadataTask = GetVideoMetadata(url);

                // Get streams
                string vstreamsTable = "";
                string astreamsTable = "";
                try
                {
                    // Get VStreams
                    Dictionary<int, Tuple<Dictionary<string, string>, VideoOnlyStreamInfo>> vstreams = new();
                    var vstreamsTask = GetVStreams(url);

                    // Get AStreams
                    Dictionary<int, Tuple<Dictionary<string, string>, AudioOnlyStreamInfo>> astreams = new();
                    var astreamsTask = GetAStreams(url);

                    // Wait for return and get result
                    await Task.WhenAll(vstreamsTask, astreamsTask);
                    vstreams = vstreamsTask.Result;
                    astreams = astreamsTask.Result;

                    // Make vstreams table
                    List<List<object>> vstreamsTableData = new();
                    foreach (var s in vstreams)
                    {
                        vstreamsTableData.Add(new()
                        {
                            s.Key,
                            s.Value.Item1["bitrate"],
                            s.Value.Item1["container"],
                            s.Value.Item1["quality"],
                            s.Value.Item1["codec"],
                            s.Value.Item1["size"],
                        });
                    }
                    vstreamsTable = ConsoleTableBuilder
                        .From(vstreamsTableData)
                        .WithColumn("ID", "Bitrate", "Container", "Quality", "Codec", "Size")
                        .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Global.TableAppearance.STREAMS)
                        .Export().ToString().Trim();

                    // Make astreams table
                    List<List<object>> astreamsTableData = new();
                    foreach (var s in astreams)
                    {
                        astreamsTableData.Add(new()
                        {
                            s.Key,
                            s.Value.Item1["bitrate"],
                            s.Value.Item1["container"],
                            s.Value.Item1["codec"],
                            s.Value.Item1["size"],
                        });
                    }
                    astreamsTable = ConsoleTableBuilder
                        .From(astreamsTableData)
                        .WithColumn("ID", "Bitrate", "Container", "Codec", "Size")
                        .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Global.TableAppearance.STREAMS)
                        .Export().ToString().Trim();
                }
                catch (VideoUnplayableException)
                {
                    vstreamsTable = TerminalOutput.Get(@"output\sources\youtube\info\video\no_vstreams.out", false, false);
                    astreamsTable = TerminalOutput.Get(@"output\sources\youtube\info\video\no_vstreams.out", false, false);
                }
                catch (AggregateException e)
                {
                    foreach (var ee in e.InnerExceptions)
                    {
                        if (ee is VideoUnplayableException)
                        {
                            vstreamsTable = TerminalOutput.Get(@"output\sources\youtube\info\video\no_vstreams.out", false, false);
                            astreamsTable = TerminalOutput.Get(@"output\sources\youtube\info\video\no_vstreams.out", false, false);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // metadataTask result
                await Task.WhenAll(metadataTask);
                Dictionary<string, string> metadata = metadataTask.Result;

                // Output
                output = TerminalOutput.Get(@"output\sources\youtube\info\video\info.out",
                    args: new()
                    {
                        metadata["title"],
                        metadata["url"],
                        metadata["author_name"],
                        metadata["views"],
                        metadata["date"],
                        metadata["duration"],
                        metadata["likes_procent"],
                        metadata["thumbnail"],
                        vstreamsTable,
                        astreamsTable,
                    }
                );
            }
            catch (HttpRequestException)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\video\error\no_internet_connection.out");
            }
            catch (VideoUnavailableException)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\video\error\video_unavailable.out");
            }
            catch (VideoRequiresPurchaseException)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\video\error\video_unavailable.out");
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\video\error\unknown.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            Console.WriteLine(output);
        }

        // WRITE INFORMATION ABOUT PLAYLIST
        public static async Task PlaylistInfo(string url, Dictionary<string, string> options)
        {
            string output = "";
            bool redirectedToVideoInfo = false;
            try
            {
                // Get playlist metadata
                var metadataTask = GetPlaylistMetadata(url);

                // Get playlist videos
                var videosTask = GetPlaylistVideos(url);

                // Wait for return and get result
                await Task.WhenAll(metadataTask, videosTask);
                var metadata = metadataTask.Result;
                var videos = videosTask.Result;

                if (options.ContainsKey("video") && int.TryParse(options["video"], out int id) && videos.ContainsKey(id))
                {
                    // Redirect to VideoInfo
                    await VideoInfo(videos[id].Url);
                    redirectedToVideoInfo = true;
                }
                else
                {
                    // Output info for the entire playlist
                    string videosData = "";
                    foreach (int i in videos.Keys)
                    {
                        videosData += String.Format("{0}: \"{1}\" ({2})\n", i, videos[i].Title, videos[i].Author.Title);
                    }
                    output = TerminalOutput.Get(@"output\sources\youtube\info\playlist\info.out",
                        args: new()
                        {
                            metadata["title"],
                            metadata["url"],
                            metadata["title"],
                            metadata["author_name"],
                            videosData.TrimEnd(),
                        }
                    );
                }
            }
            catch (HttpRequestException)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\playlist\error\no_internet_connection.out");
            }
            catch (PlaylistUnavailableException)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\playlist\error\playlist_unavailable.out");
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\sources\youtube\info\playlist\error\unknown.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            if (!(redirectedToVideoInfo)) { Console.WriteLine(output); }
        }

        #endregion MAIN_INFO



        #region MAIN_DOWNLOAD

        // DOWNLOAD VIDEO
        public static async Task VideoDownload(string url, Dictionary<string, string> options, bool async = false)
        {
            try
            {
                // Get video metadata
                var metadata = await GetVideoMetadata(url);
                string videoID = metadata["id"];

                // Options
                Dictionary<string, string> filenameCode = new()
                {
                    { "%title%", metadata["title"] },
                    { "%author%", metadata["author_name"] },
                    { "%author_id%", metadata["author_id"] },
                    { "%id%", videoID },
                    { "%duration%", metadata["duration"] },
                    { "%views%", metadata["views"] },
                    { "%pub_date%", metadata["date"] },
                    { "%act_date%", DateTime.Now.ToString(Config.Main.ReadKey("date_format")) },
                    { "%likes%", metadata["likes"] },
                    { "%dislikes%", metadata["dislikes"] },
                };
                bool onlyaudio = options.ContainsKey("onlyaudio");
                bool onlyvideo = options.ContainsKey("onlyvideo");
                string filename = Filename.Process(InputOptions.Switch("filename", options), filenameCode);
                string output_path = InputOptions.Switch("output_path", options);
                string extension;
                if (onlyaudio)
                { extension = InputOptions.Switch("audio_ext", options); }
                else
                { extension = InputOptions.Switch("video_ext", options); }



                // Downloading

                // Entry output args
                List<string> args = new()
                {
                    metadata["title"],
                    metadata["url"]
                };

                // Temporary files
                string tempOutputFile;

                // Type switch
                if (onlyvideo)
                {
                    // Get streams
                    var streams = await GetVStreams(url);
                    Dictionary<int, Dictionary<string, string>> streamsData = new(); // Get streams metadata
                    foreach (int i in streams.Keys)
                    {
                        streamsData[i] = streams[i].Item1;
                    }
                    if (options.ContainsKey("vstream") && options["vstream"] != null && int.TryParse(options["vstream"], out int id)) { } // Select stream
                    else { id = SelectBestStream(streamsData); }

                    // Add information about downloaded stream to output args
                    args.Add(String.Format("Video stream ID: {0} ({1})", id, streamsData[id]["quality"]));

                    // Start download
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\start.out", args: args));

                    // Download stream
                    if (!async)
                        Console.Write(TerminalOutput.Get(@"output\sources\universal\download.out", false, false));
                    Stopwatch downloadTime = new();
                    downloadTime.Start();
                    tempOutputFile = await DownloadVStream(streams, id, videoID);
                    downloadTime.Stop();
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                            args: new()
                            {
                                downloadTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                }
                else if (onlyaudio)
                {
                    // Get streams
                    var streams = await GetAStreams(url);
                    Dictionary<int, Dictionary<string, string>> streamsData = new(); // Get streams metadata
                    foreach (int i in streams.Keys)
                    {
                        streamsData[i] = streams[i].Item1;
                    }
                    if (options.ContainsKey("astream") && options["astream"] != null && int.TryParse(options["astream"], out int id)) { } // Select stream
                    else { id = SelectBestStream(streamsData); }

                    // Add information about downloaded stream to output args
                    args.Add(String.Format("Audio stream ID: {0} ({1})", id, streamsData[id]["bitrate"]));

                    // Start download
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\start.out", args: args));

                    // Download stream
                    if (!async)
                        Console.Write(TerminalOutput.Get(@"output\sources\universal\download.out", upSP: false, downSP: false));
                    Stopwatch downloadTime = new();
                    downloadTime.Start();
                    tempOutputFile = await DownloadAStream(streams, id, videoID);
                    downloadTime.Stop();
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                            args: new()
                            {
                                downloadTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                }
                else
                {
                    // Get streams
                    var vstreamsTask = GetVStreams(url);
                    var astreamsTask = GetAStreams(url);
                    await Task.WhenAll(vstreamsTask, astreamsTask); // Wait for astreamsTask and vstreamsTask
                    var vstreams = vstreamsTask.Result;
                    Dictionary<int, Dictionary<string, string>> vstreamsData = new(); // Get vstreams metadata
                    foreach (int i in vstreams.Keys)
                    {
                        vstreamsData[i] = vstreams[i].Item1;
                    }
                    if (options.ContainsKey("vstream") && options["vstream"] != null && int.TryParse(options["vstream"], out int v_id)) { } // Select vstream
                    else { v_id = SelectBestStream(vstreamsData); }
                    var astreams = astreamsTask.Result;
                    Dictionary<int, Dictionary<string, string>> astreamsData = new(); // Get astreams metadata
                    foreach (int i in astreams.Keys)
                    {
                        astreamsData[i] = astreams[i].Item1;
                    }
                    if (options.ContainsKey("astream") && options["astream"] != null && int.TryParse(options["astream"], out int a_id)) { } // Select astream
                    else { a_id = SelectBestStream(astreamsData); }

                    // Add information about downloaded stream to output args
                    args.Add(String.Format("Video stream ID: {0} ({1}) | Audio stream ID: {2} ({3})", v_id, vstreamsData[v_id]["quality"], a_id, astreamsData[a_id]["bitrate"]));

                    // Start download
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\start.out", args: args));

                    // Download streams
                    if (!async)
                        Console.Write(TerminalOutput.Get(@"output\sources\universal\download.out", upSP: false, downSP: false));
                    Stopwatch downloadTime = new();
                    downloadTime.Start();
                    var downloadAStreamTask = DownloadAStream(astreams, a_id, videoID);
                    var downloadVStreamTask = DownloadVStream(vstreams, v_id, videoID);
                    Task.WaitAll(downloadAStreamTask, downloadVStreamTask);
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                            args: new()
                            {
                                downloadTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                    string tempAStreamFile = downloadAStreamTask.Result;
                    string tempVStreamFile = downloadVStreamTask.Result;

                    // Merge
                    if (!async)
                        Console.Write(TerminalOutput.Get(@"output\media_processing\merge.out", upSP: false, downSP: false));
                    Stopwatch mergeTime = new();
                    mergeTime.Start();
                    tempOutputFile = MediaProcessing.MergeAV(tempVStreamFile, tempAStreamFile, videoID);
                    mergeTime.Stop();
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                            args: new()
                            {
                                mergeTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                    TempFiles.Add(tempVStreamFile);
                    TempFiles.Add(tempAStreamFile);

                    // Delete temp
                    if (!async)
                        TempFiles.DeleteAll();
                }

                // Convert
                if (Path.GetExtension(tempOutputFile).Trim('.') != extension)
                {
                    if (!async)
                        Console.Write(TerminalOutput.Get(@"output\media_processing\convert.out", upSP: false, downSP: false));
                    TempFiles.Add(tempOutputFile);
                    Stopwatch convertTime = new();
                    convertTime.Start();
                    tempOutputFile = await MediaProcessing.Convert(tempOutputFile, extension, videoID);
                    convertTime.Stop();
                    if (!async)
                        Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                            args: new()
                            {
                                convertTime.Elapsed.TotalSeconds.ToString()
                            }
                        ));
                }

                // Delete temp
                if (!async)
                    TempFiles.DeleteAll();

                // Trim start
                if ((options.ContainsKey("trim_start") && options["trim_start"] != null) || (options.ContainsKey("trim_end") && options["trim_end"] != null))
                {
                    var baseVideoDuration = TimeSpan.Parse(metadata["duration"]);
                    double startSec = 0;
                    if (TimeSpan.TryParse(options["trim_start"], out TimeSpan start) && start.TotalSeconds > 0 && start.TotalSeconds < baseVideoDuration.TotalSeconds)
                    {
                        startSec = start.TotalSeconds;
                    }
                    double endSec = baseVideoDuration.TotalSeconds;
                    if (TimeSpan.TryParse(options["trim_end"], out TimeSpan end) && end.TotalSeconds > 0 && end.TotalSeconds < baseVideoDuration.TotalSeconds)
                    {
                        startSec = end.TotalSeconds;
                    }
                    if ((startSec != 0 || endSec != baseVideoDuration.TotalSeconds) & startSec < endSec)
                    {
                        if (!async)
                            Console.Write(TerminalOutput.Get(@"output\media_processing\trim.out", upSP: false, downSP: false));
                        TempFiles.Add(tempOutputFile);
                        Stopwatch trimTime = new();
                        trimTime.Start();
                        tempOutputFile = await MediaProcessing.Trim(tempOutputFile, startSec, endSec, videoID);
                        trimTime.Stop();
                        if (!async)
                            Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                                args: new()
                                {
                                    trimTime.Elapsed.TotalSeconds.ToString()
                                }
                            ));
                    }
                }

                // Delete temp
                if (!async)
                    TempFiles.DeleteAll();

                // Move to output destination
                string outPath = String.Format(@"{0}\{1}.{2}", output_path, filename, extension);
                File.Move(tempOutputFile, outPath, true);
                if (!async)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done.out", upSP: false));
                    TempFiles.DeleteAll();
                }
            }
            catch (HttpRequestException)
            {
                if (!async)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\error\no_internet_connection.out"));
                }
                else
                {
                    throw;
                }

            }
            catch (VideoUnavailableException)
            {
                if (!async)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\error\video_unavailable.out"));
                }
                else
                {
                    throw;
                }
                
            }
            catch (VideoRequiresPurchaseException)
            {
                if (!async)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\error\video_requires_pucharse.out"));
                }
                else
                {
                    throw;
                }
            }
            catch (VideoUnplayableException)
            {
                if (!async)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\error\video_unplayable.out"));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                if (!async)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\video\error\unknown.out",
                        args: new()
                        {
                            e.Message
                        }
                    ));
                }
                else
                {
                    throw;
                }
            }
        }

        // DOWNLOAD PLAYLIST
        public static async Task PlaylistDownload(string url, Dictionary<string, string> options)
        {
            try
            {
                // Get playlist metadata
                var metadataTask = GetPlaylistMetadata(url);

                // Get playlist videos
                var videosTask = GetPlaylistVideos(url);

                // Wait for metadataTask and videosTask
                await Task.WhenAll(metadataTask, videosTask);
                var metadata = metadataTask.Result;
                var videos = videosTask.Result;

                // Start download
                Console.WriteLine(TerminalOutput.Get(
                    @"output\sources\youtube\download\playlist\start.out",
                    args: new()
                    {
                        StringC.TrimToLength(metadata["title"], 43),
                        metadata["id"],
                    }
                ));

                // Select videos to download
                IEnumerable<int> videoIds;
                if (options.ContainsKey("videos"))
                { videoIds = from id in options["videos"].Split(';') select int.Parse(id); }
                else
                { videoIds = from id in videos.Keys select id; }

                // Download videos async (redirect to VideoDownload)
                await videoIds.ParallelForEachAsync(
                    async id => 
                    {
                        // Get start cursor position
                        int positionY = Console.GetCursorPosition().Top;

                        // Start output
                        string startOutput = TerminalOutput.Get(@"output\sources\youtube\download\playlist\single\start.out", false, false,
                            args: new()
                            {
                                StringC.TrimToLength(videos[id].Title, 37),
                                videos[id].Id
                            }
                        );
                        int startOutputLength = startOutput.Length;
                        Console.WriteLine(startOutput);

                        // Start downloading
                        string endMessage;
                        Stopwatch downloadTime = new();
                        downloadTime.Start();
                        try
                        {
                            await VideoDownload(videos[id].Url, options, true);
                            downloadTime.Stop();
                            endMessage = TerminalOutput.Get(@"output\sources\universal\done_in.out", false, false,
                                args: new()
                                {
                                    downloadTime.Elapsed.Seconds.ToString()
                                }
                            );
                        }
                        catch (HttpRequestException)
                        {
                            endMessage = TerminalOutput.Get(@"output\sources\youtube\download\playlist\single\error\no_internet_connection.out", false, false);
                        }
                        catch (VideoUnavailableException)
                        {
                            endMessage = TerminalOutput.Get(@"output\sources\youtube\download\playlist\single\error\video_unavailable.out", false, false);
                        }
                        catch (VideoRequiresPurchaseException)
                        {
                            endMessage = TerminalOutput.Get(@"output\sources\youtube\download\playlist\single\error\video_unavailable.out", false, false);
                        }
                        catch (VideoUnplayableException)
                        {
                            endMessage = TerminalOutput.Get(@"output\sources\youtube\download\playlist\single\error\video_unplayable.out", false, false);
                        }
                        catch (Exception)
                        {
                            endMessage = TerminalOutput.Get(@"output\sources\youtube\download\playlist\single\error\unknown.out", false, false);
                        }

                        // Get end cursor position
                        int endPositionX = Console.GetCursorPosition().Left;
                        int endPositionY = Console.GetCursorPosition().Top;

                        // End output
                        Console.SetCursorPosition(startOutputLength, positionY);
                        Console.WriteLine(endMessage);
                        Console.SetCursorPosition(endPositionX, endPositionY);
                    },
                    maxDegreeOfParallelism: maxFilesDownloadingTasks
                );
                Console.WriteLine(TerminalOutput.Get(@"output\sources\universal\done.out"));
                TempFiles.DeleteAll();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\playlist\error\no_internet_connection.out"));
            }
            catch (PlaylistUnavailableException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\playlist\error\playlist_unavailable.out"));
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\sources\youtube\download\playlist\error\unknown.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
            }
        }

        #endregion MAIN_DOWNLOAD





        #region GET_INFO

        // GET VIDEO METADATA
        private static async Task<Dictionary<string, string>> GetVideoMetadata(string url)
        {
            // Request
            var response = await client.Videos.GetAsync(url);

            // Pack into dictionary
            Dictionary<string, string> metadata = new();
            metadata["title"] = response.Title;
            metadata["author_name"] = response.Author.Title;
            metadata["author_id"] = response.Author.ChannelId;
            metadata["date"] = response.UploadDate.Date.ToString(Config.Main.ReadKey("date_format"));
            metadata["duration"] = response.Duration.ToString();
            metadata["views"] = response.Engagement.ViewCount.ToString();
            metadata["likes"] = response.Engagement.LikeCount.ToString();
            metadata["dislikes"] = response.Engagement.DislikeCount.ToString();
            metadata["likes_procent"] = ((response.Engagement.LikeCount * 100) / (response.Engagement.LikeCount + response.Engagement.DislikeCount)).ToString();
            metadata["thumbnail"] = String.Format(@"https://img.youtube.com/vi/{0}/maxresdefault.jpg", response.Id);
            metadata["description"] = response.Description;
            metadata["url"] = response.Url;
            metadata["id"] = response.Id;

            // Return metadata
            return metadata;
        }

        // GET PLAYLIST METADATA
        private static async Task<Dictionary<string, string>> GetPlaylistMetadata(string url)
        {
            // Request
            var response = await client.Playlists.GetAsync(url);

            // Pack into dictionary
            Dictionary<string, string> metadata = new();
            metadata["title"] = response.Title;
            metadata["author_name"] = response.Author.Title;
            metadata["author_id"] = response.Author.ChannelId;
            metadata["description"] = response.Description;
            metadata["url"] = response.Url;
            metadata["id"] = response.Id;

            // Return metadata
            return metadata;
        }

        // GET PLAYLIST VIDEOS
        private static async Task<Dictionary<int, PlaylistVideo>> GetPlaylistVideos(string url)
        {
            Dictionary<int, PlaylistVideo> videos = new();
            int i = 0;
            await foreach (var v in client.Playlists.GetVideosAsync(url))
            {
                videos[i] = v;
                i++;
            }
            return videos;
        }

        #endregion GET_INFO



        #region GET_STREAMS

        // GET VSTREAMS
        private static async Task<Dictionary<int, Tuple<Dictionary<string, string>, VideoOnlyStreamInfo>>> GetVStreams(string url)
        {
            // Request
            var response = await client.Videos.Streams.GetManifestAsync(url);
            var streamObjects = response.GetVideoOnlyStreams();

            // Pack into dictionary
            Dictionary<int, Tuple<Dictionary<string, string>, VideoOnlyStreamInfo>> streams = new();
            int i = 0;
            foreach (var s in streamObjects)
            {
                Dictionary<string, string> metadata = new();
                metadata["bitrate"] = s.Bitrate.ToString();
                metadata["bitrate_base"] = s.Bitrate.BitsPerSecond.ToString();
                metadata["container"] = s.Container.Name;
                metadata["quality"] = s.VideoQuality.Label;
                metadata["codec"] = s.VideoCodec;
                metadata["size"] = s.Size.ToString();
                streams[i] = Tuple.Create(metadata, s);
                i++;
            }

            // Return streams
            return streams;
        }

        // GET ASTREAMS
        private static async Task<Dictionary<int, Tuple<Dictionary<string, string>, AudioOnlyStreamInfo>>> GetAStreams(string url)
        {
            // Request
            var response = await client.Videos.Streams.GetManifestAsync(url);
            var streamObjects = response.GetAudioOnlyStreams();

            // Pack into dictionary
            Dictionary<int, Tuple<Dictionary<string, string>, AudioOnlyStreamInfo>> streams = new();
            int i = 0;
            foreach (var s in streamObjects)
            {
                Dictionary<string, string> metadata = new();
                metadata["bitrate"] = s.Bitrate.ToString();
                metadata["bitrate_base"] = s.Bitrate.BitsPerSecond.ToString();
                metadata["container"] = s.Container.Name;
                metadata["codec"] = s.AudioCodec;
                metadata["size"] = s.Size.ToString();
                streams[i] = Tuple.Create(metadata, s);
                i++;
            }

            // Return streams
            return streams;
        }

        #endregion GET_STREAMS



        #region DOWNLOAD_STREAMS

        // DOWNLOAD VSTREAM
        private static async Task<string> DownloadVStream(Dictionary<int, Tuple<Dictionary<string, string>, VideoOnlyStreamInfo>> streams, int id, string videoID)
        {
            // Create temp directory if not exist
            Directory.CreateDirectory(Global.Paths.TEMP);

            // Temporary path
            string tempPath = String.Format(@"{0}\vstream_{1}.{2}", Global.Paths.TEMP, videoID, streams[id].Item1["container"]);

            // Download
            await client.Videos.Streams.DownloadAsync(streams[id].Item2, tempPath);

            // Return path
            return tempPath;
        }

        // DOWNLOAD ASTREAM
        private static async Task<string> DownloadAStream(Dictionary<int, Tuple<Dictionary<string, string>, AudioOnlyStreamInfo>> streams, int id, string videoID)
        {
            // Create temp directory if not exist
            Directory.CreateDirectory(Global.Paths.TEMP);

            // Temporary path
            string tempPath = String.Format(@"{0}\astream_{1}.{2}", Global.Paths.TEMP, videoID, streams[id].Item1["container"]);

            // Download
            await client.Videos.Streams.DownloadAsync(streams[id].Item2, tempPath);

            // Return path
            return tempPath;
        }

        #endregion DOWNLOAD_STREAMS



        #region OTHER

        // SELECT BEST STREAM
        private static int SelectBestStream(Dictionary<int, Dictionary<string, string>> streams)
        {
            // Max variables
            double max_bitrate_val = 0;
            int max_bitrate_id = 0;

            // Check each stream
            foreach (var s in streams)
            {
                int id = s.Key;
                double bitrate = int.Parse(s.Value["bitrate_base"]);
                // Check if max
                if (bitrate > max_bitrate_val)
                {
                    max_bitrate_val = bitrate;
                    max_bitrate_id = id;
                }
            }

            return max_bitrate_id;
        }

        #endregion OTHER
    }
}
