using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Models;
using Windows.Storage;

namespace VDownload.Core.Services.Sources.Twitch
{
    public class Vod : IVideoService
    {
        #region CONSTANTS

        // METADATA TIME FORMATS
        private static readonly string[] TimeFormats = new[]
        {
            @"h\hm\ms\s",
            @"m\ms\s",
            @"s\s",
        };

        // STREAMS RESPONSE REGULAR EXPRESSIONS
        private static readonly Regex L2Regex = new Regex(@"^#EXT-X-STREAM-INF:BANDWIDTH=\d+,CODECS=""(?<video_codec>\S+),(?<audio_codec>\S+)"",RESOLUTION=(?<width>\d+)x(?<height>\d+),VIDEO=""\w+"",FRAME-RATE=(?<frame_rate>\d+.\d+)");

        // CHUNK RESPONSE REGULAR EXPRESSION
        private static readonly Regex ChunkRegex = new Regex(@"#EXTINF:(?<duration>\d+.\d+),\n(?<filename>\S+.ts)");

        #endregion



        #region CONSTRUCTORS

        public Vod(string id)
        {
            ID = id;
        }

        #endregion



        #region PARAMETERS

        public string ID { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public DateTime Date { get; private set; }
        public TimeSpan Duration { get; private set; }
        public long Views { get; private set; }
        public Uri Thumbnail { get; private set; }
        public Stream[] Streams { get; private set; }

        #endregion



        #region STANDARD METHODS

        // GET VOD METADATA
        public async Task GetMetadataAsync()
        {
            // Get access token
            string accessToken = await Auth.ReadAccessTokenAsync();
            if (accessToken == null) throw new TwitchAccessTokenNotFoundException();

            // Check access token
            var twitchAccessTokenValidation = await Auth.ValidateAccessTokenAsync(accessToken);
            if (!twitchAccessTokenValidation.IsValid) throw new TwitchAccessTokenNotValidException();

            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Bearer {accessToken}");
            client.Headers.Add("Client-Id", Auth.ClientID);

            // Get response
            client.QueryString.Add("id", ID);
            JToken response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/videos")).GetValue("data")[0];

            // Set parameters
            Title = ((string)response["title"]).Replace("\n", "");
            Author = (string)response["user_name"];
            Date = Convert.ToDateTime(response["created_at"]);
            Duration = TimeSpan.ParseExact((string)response["duration"], TimeFormats, null);
            Views = (long)response["view_count"];
            Thumbnail = (string)response["thumbnail_url"] == string.Empty ? Globals.Assets.UnknownThumbnailImage : new Uri((string)response["thumbnail_url"]);
        }
        public void GetMetadataAsync(JToken response)
        {
            // Set parameters
            Title = ((string)response["title"]).Replace("\n", "");
            Author = (string)response["user_name"];
            Date = Convert.ToDateTime(response["created_at"]);
            Duration = TimeSpan.ParseExact((string)response["duration"], TimeFormats, null);
            Views = (long)response["view_count"];
            Thumbnail = (string)response["thumbnail_url"] == string.Empty ? Globals.Assets.UnknownThumbnailImage : new Uri((string)response["thumbnail_url"]);
        }

        // GET VOD STREAMS
        public async Task GetStreamsAsync()
        {
            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Auth.GQLApiClientID);

            // Get video GQL access token
            JToken videoAccessToken = JObject.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + ID + "\",\"playerType\":\"embed\"}}"))["data"]["videoPlaybackAccessToken"];

            // Get video streams
            string[] response = (await client.DownloadStringTaskAsync($"http://usher.twitch.tv/vod/{ID}?nauth={videoAccessToken["value"]}&nauthsig={videoAccessToken["signature"]}&allow_source=true&player=twitchweb")).Split("\n");

            // Init streams list
            List<Stream> streams = new List<Stream>();

            // Parse response
            for (int i = 2; i < response.Length; i += 3)
            {
                // Parse line 2
                Match line2 = L2Regex.Match(response[i + 1]);

                // Get info
                Uri url = new Uri(response[i + 2]);
                int width = int.Parse(line2.Groups["width"].Value);
                int height = int.Parse(line2.Groups["height"].Value);
                int frameRate = (int)Math.Round(double.Parse(line2.Groups["frame_rate"].Value));
                string videoCodec = line2.Groups["video_codec"].Value;
                string audioCodec = line2.Groups["audio_codec"].Value;

                // Create stream
                Stream stream = new Stream(url, true, StreamType.AudioVideo)
                {
                    Width = width,
                    Height = height,
                    FrameRate = frameRate,
                    VideoCodec = videoCodec,
                    AudioCodec = audioCodec,
                };

                // Add stream
                streams.Add(stream);
            }

            // Set Streams parameter
            Streams = streams.ToArray();
        }

        // DOWNLOAD AND TRANSCODE VOD
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, Stream audioVideoStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default)
        {
            // Set cancellation token
            cancellationToken.ThrowIfCancellationRequested();

            // Invoke DownloadingStarted event
            DownloadingStarted?.Invoke(this, EventArgs.Empty);

            // Get video chunks
            List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunksList = await ExtractChunksFromM3U8Async(audioVideoStream.Url);

            // Passive trim
            if ((bool)Config.GetValue("twitch_vod_passive_trim"))
            {
                var trimResult = PassiveVideoTrim(chunksList, trimStart, trimEnd, Duration);
                trimStart = trimResult.TrimStart;
                trimEnd = trimResult.TrimEnd;
            }

            // Download
            StorageFile rawFile = await downloadingFolder.CreateFileAsync("raw.ts");
            float chunksDownloaded = 0;

            Task<byte[]> downloadTask;
            Task writeTask;

            downloadTask = DownloadChunkAsync(chunksList[0].ChunkUrl);
            await downloadTask;
            for (int i = 1; i < chunksList.Count; i++)
            {
                writeTask = WriteChunkToFileAsync(rawFile, downloadTask.Result);
                downloadTask = DownloadChunkAsync(chunksList[i].ChunkUrl);
                await Task.WhenAll(writeTask, downloadTask);
                DownloadingProgressChanged(this, new ProgressChangedEventArgs((int)Math.Round(++chunksDownloaded * 100 / chunksList.Count), null));
            }
            await WriteChunkToFileAsync(rawFile, downloadTask.Result);
            DownloadingProgressChanged(this, new ProgressChangedEventArgs((int)Math.Round(++chunksDownloaded * 100 / chunksList.Count), null));

            DownloadingCompleted?.Invoke(this, EventArgs.Empty);

            // Processing
            StorageFile outputFile = await downloadingFolder.CreateFileAsync($"transcoded.{extension.ToString().ToLower()}");

            MediaProcessor mediaProcessor = new MediaProcessor(outputFile, trimStart, trimEnd);
            mediaProcessor.ProcessingStarted += ProcessingStarted;
            mediaProcessor.ProcessingProgressChanged += ProcessingProgressChanged;
            mediaProcessor.ProcessingCompleted += ProcessingCompleted;
            await mediaProcessor.Run(rawFile, extension, mediaType, cancellationToken);

            // Return output file
            return outputFile;
        }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, Stream audioVideoStream, MediaFileExtension extension, MediaType mediaType, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, audioVideoStream, extension, mediaType, new TimeSpan(0), Duration, cancellationToken); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, IVStream videoStream, VideoFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default) { throw new NotImplementedException("Twitch VOD download service doesn't support separate video and audio streams"); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, IVStream videoStream, VideoFileExtension extension, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, audioStream, videoStream, extension, new TimeSpan(0), Duration, cancellationToken); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, AudioFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default) { throw new NotImplementedException("Twitch VOD download service doesn't support separate video and audio streams"); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, AudioFileExtension extension, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, audioStream, extension, new TimeSpan(0), Duration, cancellationToken); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IVStream videoStream, VideoFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default) { throw new NotImplementedException("Twitch VOD download service doesn't support separate video and audio streams"); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IVStream videoStream, VideoFileExtension extension, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, videoStream, extension, new TimeSpan(0), Duration, cancellationToken); }

        #endregion



        #region LOCAL METHODS

        // GET CHUNKS DATA FROM M3U8 PLAYLIST
        private static async Task<List<(Uri ChunkUrl, TimeSpan ChunkDuration)>> ExtractChunksFromM3U8Async(Uri streamUrl)
        {
            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Auth.GQLApiClientID);

            // Get playlist
            string response = await client.DownloadStringTaskAsync(streamUrl);
            Debug.WriteLine(response);
            // Create dictionary
            List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunks = new List<(Uri ChunkUrl, TimeSpan ChunkDuration)>();

            // Pack data into dictionary
            foreach (Match chunk in ChunkRegex.Matches(response))
            {
                Uri chunkUrl = new Uri($"{streamUrl.AbsoluteUri.Replace(System.IO.Path.GetFileName(streamUrl.AbsoluteUri), "")}{chunk.Groups["filename"].Value}");
                TimeSpan chunkDuration = TimeSpan.FromSeconds(double.Parse(chunk.Groups["duration"].Value));
                chunks.Add((chunkUrl, chunkDuration));
            }

            // Return chunks data
            return chunks;
        }

        // PASSIVE TRIM
        private static (TimeSpan TrimStart, TimeSpan TrimEnd) PassiveVideoTrim(List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunksList, TimeSpan trimStart, TimeSpan trimEnd, TimeSpan duration)
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
            return (trimStart, trimEnd);
        }

        // DOWNLOAD CHUNK
        private static async Task<byte[]> DownloadChunkAsync(Uri chunkUrl)
        {
            int retriesCount = 0;
            while ((bool)Config.GetValue("twitch_vod_downloading_chunk_retry_after_error") && retriesCount < (int)Config.GetValue("twitch_vod_downloading_chunk_max_retries"))
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        return await client.DownloadDataTaskAsync(chunkUrl);
                    }
                }
                catch
                {
                    retriesCount++;
                    await Task.Delay((int)Config.GetValue("twitch_vod_downloading_chunk_retries_delay"));
                }
            }
            throw new WebException("An error occurs while downloading a Twitch VOD chunk");
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

        #endregion



        #region EVENT HANDLERS

        public event EventHandler DownloadingStarted;

        public event EventHandler<ProgressChangedEventArgs> DownloadingProgressChanged;

        public event EventHandler DownloadingCompleted;

        public event EventHandler ProcessingStarted;

        public event EventHandler<ProgressChangedEventArgs> ProcessingProgressChanged;

        public event EventHandler ProcessingCompleted;

        #endregion
    }
}
