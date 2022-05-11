using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services.Sources.Twitch.Helpers;
using VDownload.Core.Structs;
using Windows.Storage;

namespace VDownload.Core.Services.Sources.Twitch
{
    [Serializable]
    public class Vod : IVideo
    {
        #region CONSTRUCTORS

        public Vod(string id)
        {
            Source = VideoSource.TwitchVod;
            ID = id;
            Url = new Uri($"https://www.twitch.tv/videos/{ID}");
        }

        #endregion



        #region PROPERTIES

        public VideoSource Source { get; private set; }
        public string ID { get; private set; }
        public Uri Url { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public DateTime Date { get; private set; }
        public TimeSpan Duration { get; private set; }
        public long Views { get; private set; }
        public Uri Thumbnail { get; private set; }
        public BaseStream[] BaseStreams { get; private set; }

        #endregion



        #region PUBLIC METHODS

        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            JToken response = null;
            using (WebClient client = await Client.Helix())
            {
                client.QueryString.Add("id", ID);
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/videos")).GetValue("data")[0];
                }
                catch (WebException ex)
                {
                    if (ex.Response != null && new StreamReader(ex.Response.GetResponseStream()).ReadToEnd().Contains("Not Found")) throw new MediaNotFoundException($"Twitch VOD (ID: {ID}) was not found");
                    else if (ex.Response != null && new StreamReader(ex.Response.GetResponseStream()).ReadToEnd() == string.Empty && ex.Message.Contains("400")) throw new MediaNotFoundException($"Twitch VOD (ID: {ID}) was not found");
                    else throw;
                }
            }

            GetMetadataAsync(response);
        }
        internal void GetMetadataAsync(JToken response)
        {
            Title = ((string)response["title"]).Replace("\n", "");
            Author = (string)response["user_name"];
            Date = Convert.ToDateTime(response["created_at"]);
            Duration = ParseDuration((string)response["duration"]);
            Views = (long)response["view_count"];
            Thumbnail = (string)response["thumbnail_url"] == string.Empty ? null : new Uri(((string)response["thumbnail_url"]).Replace("%{width}", "1920").Replace("%{height}", "1080"));
        }

        public async Task GetStreamsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string[] response = null;
            using (WebClient client = Client.GQL())
            {
                cancellationToken.ThrowIfCancellationRequested();
                JToken videoAccessToken = JObject.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + ID + "\",\"playerType\":\"embed\"}}"))["data"]["videoPlaybackAccessToken"];

                cancellationToken.ThrowIfCancellationRequested();
                response = (await client.DownloadStringTaskAsync($"http://usher.twitch.tv/vod/{ID}?nauth={videoAccessToken["value"]}&nauthsig={videoAccessToken["signature"]}&allow_source=true&player=twitchweb")).Split("\n");
            }

            List<BaseStream> streams = new List<BaseStream>();

            Regex streamDataL2Regex = new Regex(@"^#EXT-X-STREAM-INF:BANDWIDTH=\d+,CODECS=""\S+,\S+"",RESOLUTION=\d+x(?<height>\d+),VIDEO=""\w+""(,FRAME-RATE=(?<frame_rate>\d+.\d+))?");

            for (int i = 2; i < response.Length; i += 3)
            {
                Match line2 = streamDataL2Regex.Match(response[i + 1]);

                BaseStream stream = new BaseStream()
                {
                    Url = new Uri(response[i + 2]),
                    Height = int.Parse(line2.Groups["height"].Value),
                    FrameRate = line2.Groups["frame_rate"].Value != string.Empty ? (int)Math.Round(double.Parse(line2.Groups["frame_rate"].Value)) : 0,
                };

                streams.Add(stream);
            }

            BaseStreams = streams.ToArray();
        }

        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, BaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TrimData trim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DownloadingProgressChanged.Invoke(this, new EventArgs.ProgressChangedEventArgs(0));
            List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunksList = await ExtractChunksFromM3U8Async(baseStream.Url, cancellationToken);

            TimeSpan duration = Duration;

            // Passive trim
            if ((bool)Config.GetValue("twitch_vod_passive_trim") && trim.Start != TimeSpan.Zero && trim.End != duration) (trim, duration) = PassiveVideoTrim(chunksList, trim, Duration);

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

            await mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trim, cancellationToken);

            // Return output file
            return outputFile;
        }

        #endregion



        #region PRIVATE METHODS

        private static async Task<List<(Uri ChunkUrl, TimeSpan ChunkDuration)>> ExtractChunksFromM3U8Async(Uri streamUrl, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string response = null;
            using (WebClient client = Client.GQL())
            {
                response = await client.DownloadStringTaskAsync(streamUrl);
            }

            List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunks = new List<(Uri ChunkUrl, TimeSpan ChunkDuration)>();

            Regex chunkDataRegex = new Regex(@"#EXTINF:(?<duration>\d+.\d+),\n(?<filename>\S+.ts)");

            string chunkLocationPath = streamUrl.AbsoluteUri.Replace(Path.GetFileName(streamUrl.AbsoluteUri), "");

            foreach (Match chunk in chunkDataRegex.Matches(response))
            {
                Uri chunkUrl = new Uri($"{chunkLocationPath}{chunk.Groups["filename"].Value}");
                TimeSpan chunkDuration = TimeSpan.FromSeconds(double.Parse(chunk.Groups["duration"].Value));
                chunks.Add((chunkUrl, chunkDuration));
            }

            return chunks;
        }

        private static (TrimData Trim, TimeSpan NewDuration) PassiveVideoTrim(List<(Uri ChunkUrl, TimeSpan ChunkDuration)> chunksList, TrimData trim, TimeSpan duration)
        {
            TimeSpan newDuration = duration;

            while (chunksList[0].ChunkDuration <= trim.Start)
            {
                trim.Start = trim.Start.Subtract(chunksList[0].ChunkDuration);
                trim.End = trim.End.Subtract(chunksList[0].ChunkDuration);
                newDuration = newDuration.Subtract(chunksList[0].ChunkDuration);
                chunksList.RemoveAt(0);
            }

            while (chunksList.Last().ChunkDuration <= newDuration.Subtract(trim.End))
            {
                newDuration = newDuration.Subtract(chunksList.Last().ChunkDuration);
                chunksList.RemoveAt(chunksList.Count - 1);
            }

            return (trim, newDuration);
        }

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

        private static Task WriteChunkToFileAsync(StorageFile file, byte[] chunk)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var stream = new FileStream(file.Path, FileMode.Append))
                {
                    stream.Write(chunk, 0, chunk.Length);
                    stream.Close();
                }
            });
        }

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



        #region EVENTS

        public event EventHandler<EventArgs.ProgressChangedEventArgs> DownloadingProgressChanged;
        public event EventHandler<EventArgs.ProgressChangedEventArgs> ProcessingProgressChanged;

        #endregion
    }
}
