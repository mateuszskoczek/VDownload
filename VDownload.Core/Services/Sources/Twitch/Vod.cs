using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using VDownload.Core.Services.Sources.Twitch.Helpers;
using VDownload.Core.Structs;
using Windows.Storage;

namespace VDownload.Core.Services.Sources.Twitch
{
    public class Vod : IVideoService
    {
        #region CONSTRUCTORS

        public Vod(string id)
        {
            ID = id;
        }

        #endregion



        #region PROPERTIES

        public string ID { get; private set; }
        public Uri VideoUrl { get; private set; }
        public Metadata Metadata { get; private set; }
        public BaseStream[] BaseStreams { get; private set; }

        #endregion



        #region STANDARD METHODS

        // GET VOD METADATA
        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get response
            JToken response = null;
            using (WebClient client = await Client.Helix())
            {
                client.QueryString.Add("id", ID);
                cancellationToken.ThrowIfCancellationRequested();
                response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/videos")).GetValue("data")[0];
            }

            // Set parameters
            GetMetadataAsync(response);
        }
        internal void GetMetadataAsync(JToken response)
        {
            // Create unified video url
            VideoUrl = new Uri($"https://www.twitch.tv/videos/{ID}");

            // Set metadata
            Metadata = new Metadata()
            {
                Title = ((string)response["title"]).Replace("\n", ""),
                Author = (string)response["user_name"],
                Date = Convert.ToDateTime(response["created_at"]),
                Duration = ParseDuration((string)response["duration"]),
                Views = (long)response["view_count"],
                Thumbnail = (string)response["thumbnail_url"] == string.Empty ? null : new Uri(((string)response["thumbnail_url"]).Replace("%{width}", "1920").Replace("%{height}", "1080")),
            };
        }

        // GET VOD STREAMS
        public async Task GetStreamsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get response
            string[] response = null;
            using (WebClient client = Client.GQL())
            {
                // Get video GQL access token
                cancellationToken.ThrowIfCancellationRequested();
                JToken videoAccessToken = JObject.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + ID + "\",\"playerType\":\"embed\"}}"))["data"]["videoPlaybackAccessToken"];

                // Get video streams
                cancellationToken.ThrowIfCancellationRequested();
                response = (await client.DownloadStringTaskAsync($"http://usher.twitch.tv/vod/{ID}?nauth={videoAccessToken["value"]}&nauthsig={videoAccessToken["signature"]}&allow_source=true&player=twitchweb")).Split("\n");
            }

            // Init streams list
            List<BaseStream> streams = new List<BaseStream>();

            // Stream data line2 regular expression
            Regex streamDataL2Regex = new Regex(@"^#EXT-X-STREAM-INF:BANDWIDTH=\d+,CODECS=""\S+,\S+"",RESOLUTION=\d+x(?<height>\d+),VIDEO=""\w+""(,FRAME-RATE=(?<frame_rate>\d+.\d+))?");

            // Parse response
            for (int i = 2; i < response.Length; i += 3)
            {
                // Parse line 2
                Match line2 = streamDataL2Regex.Match(response[i + 1]);

                // Create stream
                BaseStream stream = new BaseStream()
                {
                    Url = new Uri(response[i + 2]),
                    Height = int.Parse(line2.Groups["height"].Value),
                    FrameRate = line2.Groups["frame_rate"].Value != string.Empty ? (int)Math.Round(double.Parse(line2.Groups["frame_rate"].Value)) : 0,
                };

                // Add stream
                streams.Add(stream);
            }

            // Set streams
            BaseStreams = streams.ToArray();
        }

        // DOWNLOAD AND TRANSCODE VOD
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, BaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default)
        {
            // Invoke DownloadingStarted event
            cancellationToken.ThrowIfCancellationRequested();
            DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

            // Get video chunks
            cancellationToken.ThrowIfCancellationRequested();
            List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunksList = await ExtractChunksFromM3U8Async(baseStream.Url, cancellationToken);

            // Changeable duration
            TimeSpan duration = Metadata.Duration;

            // Passive trim
            if ((bool)Config.GetValue("twitch_vod_passive_trim") && trimStart != TimeSpan.Zero && trimEnd != duration) (trimStart, trimEnd, duration) = PassiveVideoTrim(chunksList, trimStart, trimEnd, Metadata.Duration);

            // Download
            cancellationToken.ThrowIfCancellationRequested();
            StorageFile rawFile = await downloadingFolder.CreateFileAsync("raw.ts");

            double chunksDownloaded = 0;

            Task<byte[]> downloadTask;
            Task writeTask;

            cancellationToken.ThrowIfCancellationRequested();
            downloadTask = DownloadChunkAsync(chunksList[0].ChunkUrl);
            await downloadTask;
            for (int i = 1; i < chunksList.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                writeTask = WriteChunkToFileAsync(rawFile, downloadTask.Result);
                downloadTask = DownloadChunkAsync(chunksList[i].ChunkUrl);
                await Task.WhenAll(writeTask, downloadTask);
                DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(++chunksDownloaded * 100 / chunksList.Count));
            }
            cancellationToken.ThrowIfCancellationRequested();
            await WriteChunkToFileAsync(rawFile, downloadTask.Result);
            DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(100, true));

            // Processing
            cancellationToken.ThrowIfCancellationRequested();
            StorageFile outputFile = await downloadingFolder.CreateFileAsync($"transcoded.{extension.ToString().ToLower()}");

            MediaProcessor mediaProcessor = new MediaProcessor();
            mediaProcessor.ProgressChanged += ProcessingProgressChanged;

            Task mediaProcessorTask;
            if (trimStart == TimeSpan.Zero && trimEnd == duration) mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, cancellationToken: cancellationToken);
            else if (trimStart == TimeSpan.Zero) mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trimStart: trimStart, cancellationToken: cancellationToken);
            else if (trimEnd == duration) mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trimEnd: trimEnd, cancellationToken: cancellationToken);
            else mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trimStart, trimEnd, cancellationToken);
            await mediaProcessorTask;

            // Return output file
            return outputFile;
        }

        #endregion



        #region LOCAL METHODS

        // GET CHUNKS DATA FROM M3U8 PLAYLIST
        private static async Task<List<(Uri ChunkUrl, TimeSpan ChunkDuration)>> ExtractChunksFromM3U8Async(Uri streamUrl, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get response
            string response = null;
            using (WebClient client = Client.GQL())
            {
                response = await client.DownloadStringTaskAsync(streamUrl);
            }

            // Create dictionary
            List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunks = new List<(Uri ChunkUrl, TimeSpan ChunkDuration)>();

            // Chunk data regular expression
            Regex chunkDataRegex = new Regex(@"#EXTINF:(?<duration>\d+.\d+),\n(?<filename>\S+.ts)");

            // Chunks location
            string chunkLocationPath = streamUrl.AbsoluteUri.Replace(System.IO.Path.GetFileName(streamUrl.AbsoluteUri), "");

            // Pack data into dictionary
            foreach (Match chunk in chunkDataRegex.Matches(response))
            {
                Uri chunkUrl = new Uri($"{chunkLocationPath}{chunk.Groups["filename"].Value}");
                TimeSpan chunkDuration = TimeSpan.FromSeconds(double.Parse(chunk.Groups["duration"].Value));
                chunks.Add((chunkUrl, chunkDuration));
            }

            // Return chunks data
            return chunks;
        }

        // PASSIVE TRIM
        private static (TimeSpan NewTrimStart, TimeSpan NewTrimEnd, TimeSpan NewDuration) PassiveVideoTrim(List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunksList, TimeSpan trimStart, TimeSpan trimEnd, TimeSpan duration)
        {
            // Copy duration
            TimeSpan newDuration = duration;

            // Trim at start
            while (chunksList[0].ChunkDuration <= trimStart)
            {
                trimStart = trimStart.Subtract(chunksList[0].ChunkDuration);
                trimEnd = trimEnd.Subtract(chunksList[0].ChunkDuration);
                newDuration = newDuration.Subtract(chunksList[0].ChunkDuration);
                chunksList.RemoveAt(0);
            }

            // Trim at end
            while (chunksList.Last().ChunkDuration <= newDuration.Subtract(trimEnd))
            {
                newDuration = newDuration.Subtract(chunksList.Last().ChunkDuration);
                chunksList.RemoveAt(chunksList.Count - 1);
            }

            // Return data
            return (trimStart, trimEnd, newDuration);
        }

        // DOWNLOAD CHUNK
        private static async Task<byte[]> DownloadChunkAsync(Uri chunkUrl, CancellationToken cancellationToken = default)
        {
            int retriesCount = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        return await client.DownloadDataTaskAsync(chunkUrl);
                    }
                }
                catch (WebException wex)
                {
                    if ((bool)Config.GetValue("twitch_vod_downloading_chunk_retry_after_error") && retriesCount < (int)Config.GetValue("twitch_vod_downloading_chunk_max_retries"))
                    {
                        retriesCount++;
                        await Task.Delay((int)Config.GetValue("twitch_vod_downloading_chunk_retries_delay"));
                    }
                    else throw wex;
                }
            }
        }

        // WRITE CHUNK TO FILE
        private static Task WriteChunkToFileAsync(StorageFile file, byte[] chunk)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var stream = new System.IO.FileStream(file.Path, System.IO.FileMode.Append))
                {
                    stream.Write(chunk, 0, chunk.Length);
                    stream.Close();
                }
            });
        }

        // PARSE DURATION
        private static TimeSpan ParseDuration(string duration)
        {
            char[] separators = { 'h', 'm', 's' };
            string[] durationParts = duration.Split(separators, StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();

            TimeSpan timeSpan = new TimeSpan(
                durationParts.Count() > 2 ? int.Parse(durationParts[2]) : 0,
                durationParts.Count() > 1 ? int.Parse(durationParts[1]) : 0,
                int.Parse(durationParts[0]));

            return timeSpan;
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<EventArgs.ProgressChangedEventArgs> DownloadingProgressChanged;
        public event EventHandler<EventArgs.ProgressChangedEventArgs> ProcessingProgressChanged;

        #endregion
    }
}
