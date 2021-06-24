using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using YoutubeExplode;
using YoutubeExplode.Exceptions;
using YoutubeExplode.Videos.Streams;
using ConsoleTableExt;
using VDownload.Parsers;

namespace VDownload
{
    class Youtube
    {
        // YOUTUBE CLIENT
        private static readonly YoutubeClient Client = new();




        // INFORMATIONS ABOUT VIDEO (METADATA, VIDEO AND AUDIO STREAMS)
        public static void VideoInfo(string url)
        {
            string output;
            try
            {
                // Get metadata
                var metadata = GetVideoMetadata(url);

                // Get VStreams
                string VStreamsTable;
                try
                {
                    var VStreams = GetVStreams(url);
                    List<List<object>> VStreamsTableData = new();
                    foreach (var e in VStreams)
                    {
                        VStreamsTableData.Add(new()
                            {
                                e.Key,
                                e.Value.Item1[0],
                                e.Value.Item1[1],
                                e.Value.Item1[2],
                                e.Value.Item1[3],
                                e.Value.Item1[4]
                            }
                        );
                    }
                    VStreamsTable = ConsoleTableBuilder
                        .From(VStreamsTableData)
                        .WithColumn("ID", "Bitrate", "Container", "Quality", "Codec", "Size")
                        .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Global.TableAppearance.STREAMS)
                        .Export().ToString().Trim();
                }
                catch (VideoUnplayableException)
                {
                    VStreamsTable = TerminalOutput.Get(@"output\youtube\info\video\no_vstreams.out", false, false);
                }

                // Get AStreams
                string AStreamsTable;
                try
                {
                    var AStreams = GetAStreams(url);
                    List<List<object>> AStreamsTableData = new();
                    foreach (var e in AStreams)
                    {
                        AStreamsTableData.Add(new() 
                            {
                                e.Key,
                                e.Value.Item1[0],
                                e.Value.Item1[1],
                                e.Value.Item1[2],
                                e.Value.Item1[3]
                            }
                        );
                    }
                    AStreamsTable = ConsoleTableBuilder
                        .From(AStreamsTableData)
                        .WithColumn("ID", "Bitrate", "Container", "Codec", "Size")
                        .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Global.TableAppearance.STREAMS)
                        .Export().ToString().Trim();
                }
                catch (VideoUnplayableException)
                {
                    AStreamsTable = TerminalOutput.Get(@"output\youtube\info\video\no_vstreams.out", false, false);
                }

                // Output
                output = TerminalOutput.Get(@"output\youtube\info\video\info.out",
                    args: new()
                    {
                        metadata["title"],
                        metadata["url"],
                        metadata["author"],
                        metadata["views"],
                        metadata["date"],
                        metadata["duration"],
                        metadata["rating"],
                        metadata["thumbnail"],
                        VStreamsTable,
                        AStreamsTable,
                    }
                );
            }
            catch (VideoUnavailableException)
            {
                output = TerminalOutput.Get(@"output\youtube\info\video\error\video_unavailable.out");
            }
            catch (VideoRequiresPurchaseException)
            {
                output = TerminalOutput.Get(@"output\youtube\info\video\error\video_requires_pucharse.out");
            }
            catch (HttpRequestException)
            {
                output = TerminalOutput.Get(@"output\youtube\info\video\error\no_internet_connection.out");
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\youtube\info\video\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            Console.WriteLine(output);
        }


        // INFORMATION ABOUT PLAYLIST
        public static void PlaylistInfo(string url, Dictionary<string, string> options)
        {
            string output = "";
            bool redirectedToVideoInfo = false;
            try
            {
                // Get metadata
                var metadata = GetPlaylistMetadata(url);

                // Get video list
                var videos = GetPlaylistVideos(url).Result;

                if (options.ContainsKey("video") && int.TryParse(options["video"], out int id) && videos.ContainsKey(id))
                {
                    // Redirect to VideoInfo
                    string videoUrl = videos[id][2];
                    VideoInfo(videoUrl);
                    redirectedToVideoInfo = true;
                }
                else
                {
                    // Output info for the entire playlist
                    string videosData = "";
                    foreach (int i in videos.Keys)
                    {
                        videosData += String.Format("{0}: \"{1}\" ({2})\n", i, videos[i][0], videos[i][1]);
                    }
                    output = TerminalOutput.Get(@"output\youtube\info\playlist\info.out",
                        args: new()
                        {
                            metadata["title"],
                            metadata["url"],
                            metadata["title"],
                            metadata["author"],
                            videosData.TrimEnd(),
                        }
                    );
                }
            }
            catch (PlaylistUnavailableException)
            { 
                output = TerminalOutput.Get(@"output\youtube\info\playlist\error\playlist_unavailable.out");
            }
            catch (HttpRequestException)
            {
                output = TerminalOutput.Get(@"output\youtube\info\playlist\error\no_internet_connection.out");
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\youtube\info\playlist\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            if (!(redirectedToVideoInfo)) { Console.WriteLine(output); }
        }




        // DOWNLOAD VIDEO
        public static void VideoDownload(string url, Dictionary<string, string> options)
        {
            try
            {
                // Get metadata
                var metadata = GetVideoMetadata(url);

                // Options
                Dictionary<string, string> filenameCode = new();
                filenameCode["%title%"] = metadata["title"];
                filenameCode["%author%"] = metadata["author"];
                filenameCode["%id%"] = metadata["id"];
                filenameCode["%duration%"] = metadata["duration"];
                filenameCode["%views%"] = metadata["views"];
                filenameCode["%pub_date%"] = metadata["date"];
                filenameCode["%act_date%"] = DateTime.Now.ToString(Config.Main.ReadKey("date_format"));
                
                string filename = Config.Main.ReadKey("filename");
                if (options.ContainsKey("filename") && !(options["filename"] == null))
                {
                    filename = options["filename"];
                }
                filename = Filename.Get(filename, filenameCode);
                string output_path = Config.Main.ReadKey("output_path");
                if (options.ContainsKey("output_path") && !(options["output_path"] == null))
                {
                    output_path = options["output_path"];
                }
                string video_ext = Config.Main.ReadKey("video_ext");
                if (options.ContainsKey("video_ext") && !(options["video_ext"] == null))
                {
                    video_ext = options["video_ext"];
                }
                string audio_ext = Config.Main.ReadKey("audio_ext");
                if (options.ContainsKey("audio_ext") && !(options["audio_ext"] == null))
                {
                    audio_ext = options["audio_ext"];
                }
                bool onlyvideo = options.ContainsKey("onlyvideo");
                bool onlyaudio = options.ContainsKey("onlyaudio");

                // Downloading
                List<string> args = new()
                {
                    metadata["title"],
                    metadata["url"]
                };
                List<string> tempFiles = new();
                if (onlyvideo)
                {
                    // Get VStreams
                    var streams = GetVStreams(url);
                    Dictionary<int, string[]> streamsData = new();
                    foreach (int i in streams.Keys)
                    {
                        streamsData[i] = streams[i].Item1;
                    }

                    // Select stream
                    if (options.ContainsKey("video") && options["video"] != null && int.TryParse(options["video"], out int id)) { }
                    else { id = SelectBestStream(streamsData); }

                    // Add information about downloaded stream to output args
                    args.Add(String.Format("Video stream ID: {0} ({1})", id, streamsData[id][2]));

                    // Start download
                    Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\start.out", args: args));
                    string tempPath = DownloadVStream(streams, id); // Own output
                    string outPath = String.Format(@"{0}\{1}.{2}", output_path, filename, video_ext);

                    if (Path.GetExtension(tempPath).Trim('.') == video_ext)
                    {
                        // Move and rename temporary video file to output
                        File.Move(tempPath, outPath, true);
                    }
                    else
                    {
                        // Process media
                        MediaProcessing.Convert(tempPath, outPath); // Own output
                        tempFiles.Add(tempPath);
                    }
                }
                else if (onlyaudio)
                {
                    // Get AStreams
                    var streams = GetAStreams(url);
                    Dictionary<int, string[]> streamsData = new();
                    foreach (int i in streams.Keys)
                    {
                        streamsData[i] = streams[i].Item1;
                    }

                    // Select stream
                    if (options.ContainsKey("audio") && options["audio"] != null && int.TryParse(options["audio"], out int id)) { }
                    else { id = SelectBestStream(streamsData); }

                    // Add information about downloaded stream to output args
                    args.Add(String.Format("Audio stream ID: {0} ({1})", id, streamsData[id][0]));

                    // Start download
                    Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\start.out", args: args));
                    string tempPath = DownloadAStream(streams, id); // Own output
                    string outPath = String.Format(@"{0}\{1}.{2}", output_path, filename, audio_ext);

                    if (Path.GetExtension(tempPath).Trim('.') == audio_ext)
                    {
                        // Move and rename temporary audio file to output
                        File.Move(tempPath, outPath, true);
                    }
                    else
                    {
                        // Process media
                        MediaProcessing.Convert(tempPath, outPath); // Own output
                        tempFiles.Add(tempPath);
                    }
                }
                else
                {
                    // Get VStreams
                    var streamsVideo = GetVStreams(url);
                    Dictionary<int, string[]> streamsVideoData = new();
                    foreach (int i in streamsVideo.Keys)
                    {
                        streamsVideoData[i] = streamsVideo[i].Item1;
                    }

                    // Select VStream
                    if (options.ContainsKey("video") && options["video"] != null && int.TryParse(options["video"], out int video_id)) { }
                    else { video_id = SelectBestStream(streamsVideoData); }

                    // Get AStreams
                    var streamsAudio = GetAStreams(url);
                    Dictionary<int, string[]> streamsAudioData = new();
                    foreach (int i in streamsAudio.Keys)
                    {
                        streamsAudioData[i] = streamsAudio[i].Item1;
                    }

                    // Select AStream
                    if (options.ContainsKey("audio") && options["audio"] != null && int.TryParse(options["audio"], out int audio_id)) { }
                    else { audio_id = SelectBestStream(streamsAudioData); }

                    // Add information about downloaded stream to output args
                    args.Add(String.Format("Video stream ID: {0} ({1}) | Audio stream ID: {2} ({3})", video_id, streamsVideoData[video_id][2], audio_id, streamsAudioData[audio_id][0]));

                    // Start download
                    Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\start.out", args: args));
                    string tempVideoPath = DownloadVStream(streamsVideo, video_id); // Own output
                    string tempAudioPath = DownloadAStream(streamsAudio, audio_id); // Own output
                    string outPath = String.Format(@"{0}\{1}.{2}", output_path, filename, video_ext);

                    // Process media
                    MediaProcessing.Merge(tempVideoPath, tempAudioPath, outPath);
                    tempFiles.Add(tempVideoPath);
                    tempFiles.Add(tempAudioPath);
                }

                // Delete temporary files
                if (tempFiles.Count > 0)
                {
                    Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\delete_temporary.out", upSP: false, downSP: false));
                    foreach (string p in tempFiles)
                    {
                        File.Delete(p);
                    }
                }
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\done.out", upSP: false));
            }
            catch (VideoUnavailableException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\error\video_unavailable.out"));
            }
            catch (VideoRequiresPurchaseException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\error\video_requires_pucharse.out"));
            }
            catch (VideoUnplayableException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\error\video_unplayable.out"));
            }
            catch (HttpRequestException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\error\no_internet_connection.out"));
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\video\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
            }
        }


        // DOWNLOAD PLAYLIST
        public static void PlaylistDownload(string url, Dictionary<string, string> options)
        {
            try
            {
                // Get metadata
                var playlistMetadata = GetPlaylistMetadata(url);

                // Get video list
                var playlistVideos = GetPlaylistVideos(url).Result;

                // Start download
                Console.WriteLine(TerminalOutput.Get(
                    @"output\youtube\download\playlist\start.out",
                    args: new()
                    {
                        playlistMetadata["title"],
                        playlistMetadata["url"],
                    }
                ));

                // Select videos to download
                IEnumerable<int> videoIds;
                if (options.ContainsKey("playlist_id"))
                {
                    videoIds = from id in options["playlist_id"].Split(';') select int.Parse(id);
                }
                else
                {
                    videoIds = from id in playlistVideos.Keys select id;
                }

                // Redirect to VideoDownload
                foreach (int id in videoIds)
                {
                    VideoDownload(playlistVideos[id][2], options);
                }
            }
            catch (PlaylistUnavailableException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\playlist\error\playlist_unavailable.out"));
            }
            catch (HttpRequestException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\playlist\error\no_internet_connection.out"));
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\playlist\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
            }
        }






        // DOWNLOAD VSTREAM
        private static string DownloadVStream(Dictionary<int, Tuple<string[], VideoOnlyStreamInfo>> streams, int id)
        {
            // Create temp directory if not exist
            try
            {
                Directory.CreateDirectory(Global.Paths.TEMP);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\vstreams\error\temp_dir_access_denied.out"));
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\vstreams\error\temp_dir_undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
                Environment.Exit(0);
            }

            // Temporary path
            string tempPath = String.Format(@"{0}\video.{1}", Global.Paths.TEMP, streams[id].Item1[1]);

            // Start download
            Console.Write(TerminalOutput.Get(@"output\youtube\download\vstreams\download.out", upSP: false, downSP: false));
            Stopwatch downloadTime = new();
            downloadTime.Start();
            Client.Videos.Streams.DownloadAsync(streams[id].Item2, tempPath).AsTask().Wait();
            downloadTime.Stop();
            Console.WriteLine(String.Format(" (Done in {0} seconds)", downloadTime.Elapsed.TotalSeconds));

            return tempPath;
        }


        // DOWNLOAD ASTREAMS
        private static string DownloadAStream(Dictionary<int, Tuple<string[], AudioOnlyStreamInfo>> streams, int id)
        {
            // Create temp directory if not exist
            try
            {
                Directory.CreateDirectory(Global.Paths.TEMP);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\astreams\error\temp_dir_access_denied.out"));
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\youtube\download\astreams\error\temp_dir_undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
                Environment.Exit(0);
            }

            // Temporary path
            string tempPath = String.Format(@"{0}\audio.{1}", Global.Paths.TEMP, streams[id].Item1[1]);

            // Start download
            Console.Write(TerminalOutput.Get(@"output\youtube\download\astreams\download.out", upSP: false, downSP: false));
            var downloadTime = new Stopwatch();
            downloadTime.Start();
            Client.Videos.Streams.DownloadAsync(streams[id].Item2, tempPath).AsTask().Wait();
            downloadTime.Stop();
            Console.WriteLine(String.Format(" (Done in {0} seconds)", downloadTime.Elapsed.TotalSeconds));

            return tempPath;
        }





        // GET VIDEO METADATA
        private static Dictionary<string, string> GetVideoMetadata(string url)
        {
            // Get metadata
            var video = Client.Videos.GetAsync(url).Result;

            Dictionary<string, string> metadata = new();
            metadata["title"] = video.Title;
            metadata["author"] = video.Author.Title;
            metadata["date"] = video.UploadDate.Date.ToString(Config.Main.ReadKey("date_format"));
            metadata["duration"] = video.Duration.ToString();
            metadata["views"] = video.Engagement.ViewCount.ToString();
            metadata["rating"] = String.Format("{0}/{1}", video.Engagement.LikeCount.ToString(), video.Engagement.DislikeCount.ToString());
            metadata["thumbnail"] = String.Format(@"https://img.youtube.com/vi/{0}/maxresdefault.jpg", video.Id);
            metadata["url"] = video.Url;
            metadata["id"] = video.Id;

            return metadata;
        }


        // GET PLAYLIST METADATA
        private static Dictionary<string, string> GetPlaylistMetadata(string url)
        {
            // Get metadata
            var playlist = Client.Playlists.GetAsync(url).Result;

            Dictionary<string, string> metadata = new();
            metadata["title"] = playlist.Title;
            metadata["author"] = playlist.Author.Title;
            metadata["url"] = playlist.Url;
            metadata["id"] = playlist.Id;

            return metadata;
        }


        

        // GET VSTREAMS
        private static Dictionary<int, Tuple<string[], VideoOnlyStreamInfo>> GetVStreams(string url)
        {
            // Get VStreams
            var streamObjects = Client.Videos.Streams.GetManifestAsync(url).Result.GetVideoOnlyStreams();

            Dictionary<int, Tuple<string[], VideoOnlyStreamInfo>> streams = new();
            int i = 0;
            foreach (var s in streamObjects)
            {
                string[] metadata = {
                    s.Bitrate.ToString(),
                    s.Container.ToString(),
                    s.VideoQuality.ToString(),
                    s.VideoCodec,
                    s.Size.ToString(),
                };
                var stream = Tuple.Create(metadata, s);
                streams[i] = stream;
                i++;
            }

            return streams;
        }


        // GET ASTREAMS
        private static Dictionary<int, Tuple<string[], AudioOnlyStreamInfo>> GetAStreams(string url)
        {
            // Get AStreams
            var streamObjects = Client.Videos.Streams.GetManifestAsync(url).Result.GetAudioOnlyStreams();

            Dictionary<int, Tuple<string[], AudioOnlyStreamInfo>> streams = new();
            int i = 0;
            foreach (var s in streamObjects)
            {
                string[] metadata = {
                    s.Bitrate.ToString(),
                    s.Container.ToString(),
                    s.AudioCodec,
                    s.Size.ToString(),
                };
                var stream = Tuple.Create(metadata, s);
                streams[i] = stream;
                i++;
            }

            return streams;
        }




        // GET PLAYLIST VIDEOS
        private static async Task<Dictionary<int, string[]>> GetPlaylistVideos(string url)
        {
            Dictionary<int, string[]> videos = new();
            int i = 0;
            await foreach (var v in Client.Playlists.GetVideosAsync(url))
            {
                string[] metadata = {
                    v.Title,
                    v.Author.Title,
                    v.Url
                };
                videos[i] = metadata;
                i++;
            }
            return videos;
        }




        // SELECT BEST STREAM
        private static int SelectBestStream(Dictionary<int, string[]> streams)
        {
            // Max variables
            double max_bitrate_val = 0;
            int max_bitrate_id = 0;

            // Check each stream
            foreach (var s in streams)
            {
                int id = s.Key;
                string bitrate = s.Value[0];
                double bitrate_val = Double.Parse(bitrate.Split(' ')[0]);
                string bitrate_unit = bitrate.Split(' ')[1];

                // Convert to bit/s
                switch (bitrate_unit)
                {
                    case "Kbit/s": bitrate_val *= Math.Pow(1000, 1); break;
                    case "Mbit/s": bitrate_val *= Math.Pow(1000, 2); break;
                    case "Gbit/s": bitrate_val *= Math.Pow(1000, 3); break;
                    case "Tbit/s": bitrate_val *= Math.Pow(1000, 4); break;
                    case "Pbit/s": bitrate_val *= Math.Pow(1000, 5); break;
                    case "Ebit/s": bitrate_val *= Math.Pow(1000, 6); break;
                    case "Zbit/s": bitrate_val *= Math.Pow(1000, 7); break;
                    case "Ybit/s": bitrate_val *= Math.Pow(1000, 8); break;
                }

                // Check if max
                if (bitrate_val > max_bitrate_val)
                {
                    max_bitrate_val = bitrate_val;
                    max_bitrate_id = id;
                }
            }

            return max_bitrate_id;
        }
    }
}