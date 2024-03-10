using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Sources.Twitch.Models.Internal;

namespace VDownload.Sources.Twitch.Models
{
    public class TwitchVodStream : VideoStream
    {
        #region SERVICES

        protected readonly HttpClient _httpClient;

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region PROPERTIES

        public string UrlM3U8 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string VideoCodec { get; set; }
        public string AudioCodec { get; set; }

        #endregion



        #region CONSTRUCTORS

        public TwitchVodStream(HttpClient httpClient, IConfigurationService configurationService, ISettingsService settingsService)
        {
            _httpClient = httpClient;

            _configurationService = configurationService;
            _settingsService = settingsService;
        }

        #endregion



        #region PUBLIC METHODS

        public async override Task<VideoStreamDownloadResult> Download(string taskTemporaryDirectory, IProgress<double> onProgress, CancellationToken token, TimeSpan duration, TimeSpan trimStart, TimeSpan trimEnd)
        {
            token.ThrowIfCancellationRequested();

            string m3u8 = await _httpClient.GetStringAsync(UrlM3U8, token);

            token.ThrowIfCancellationRequested();

            string m3u8BaseUrl = Path.GetDirectoryName(UrlM3U8).Replace("https:\\", "https://").Replace("http:\\", "http://").Replace('\\', '/');
            Regex regex = new Regex(_configurationService.Twitch.Download.Vod.ChunkRegex);
            MatchCollection matches = regex.Matches(m3u8);
            long index = 0;

            List<TwitchVodChunk> chunks = new List<TwitchVodChunk>();
            foreach (Match match in matches)
            {
                token.ThrowIfCancellationRequested();

                string filename = match.Groups["file"].Value;
                string durationString = match.Groups["duration"].Value;

                TimeSpan chunkDuration = TimeSpan.FromSeconds(double.Parse(durationString, CultureInfo.InvariantCulture));
                string url = $"{m3u8BaseUrl}/{filename}";
                string location = Path.Combine(taskTemporaryDirectory, filename);

                chunks.Add(new TwitchVodChunk
                {
                    Url = url,
                    Index = index,
                    Duration = chunkDuration,
                    Location = location
                });

                index++;
            }

            token.ThrowIfCancellationRequested();

            if (_settingsService.Data.Twitch.Vod.PassiveTrimming)
            {
                PassiveTrimming(chunks, ref trimStart, ref trimEnd, ref duration);
            }

            token.ThrowIfCancellationRequested();

            long downloadedCount = 0;
            Action taskEnd = () =>
            {
                downloadedCount++;
                double progress = ((double)downloadedCount / chunks.Count) * 100;
                onProgress.Report(progress);
            };
            ActionBlock<TwitchVodChunk> block = new ActionBlock<TwitchVodChunk>(x => DownloadChunk(x, token, taskEnd), new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _settingsService.Data.Twitch.Vod.MaxNumberOfParallelDownloads
            });
            foreach (TwitchVodChunk chunk in chunks)
            {
                block.Post(chunk);
            }
            block.Complete();
            await block.Completion;

            token.ThrowIfCancellationRequested();

            string file = Path.Combine(taskTemporaryDirectory, _configurationService.Twitch.Download.Vod.FileName);
            MergeFiles(file, chunks.Select(x => x.Location), token, true);

            return new VideoStreamDownloadResult
            {
                File = file,
                NewTrimStart = trimStart,
                NewTrimEnd = trimEnd,
                NewDuration = duration,
            };
        }

        #endregion



        #region PRIVATE METHODS

        private void MergeFiles(string destinationPath, IEnumerable<string> sourceFiles, CancellationToken token, bool deleteSource = false)
        {
            using (FileStream outputStream = File.Create(destinationPath))
            {
                foreach (string path in sourceFiles)
                {
                    token.ThrowIfCancellationRequested();

                    using (FileStream inputStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        inputStream.CopyTo(outputStream);
                    }
                }
            }
            foreach (string item in sourceFiles)
            {
                if (deleteSource)
                {
                    File.Delete(item);
                }
            }
        }

        private void PassiveTrimming(List<TwitchVodChunk> chunks, ref TimeSpan trimStart, ref TimeSpan trimEnd, ref TimeSpan duration)
        {
            while (chunks.First().Duration <= trimStart)
            {
                TwitchVodChunk chunk = chunks.First();
                TimeSpan chunkDuration = chunk.Duration;
                trimStart -= chunkDuration;
                trimEnd -= chunkDuration;
                duration -= chunkDuration;
                chunks.Remove(chunk);
            }

            while (chunks.Last().Duration <= duration.Subtract(trimEnd))
            {
                TwitchVodChunk chunk = chunks.Last();
                TimeSpan chunkDuration = chunk.Duration;
                duration -= chunkDuration;
                chunks.Remove(chunk);
            }
        }

        private async Task DownloadChunk(TwitchVodChunk chunk, CancellationToken token, Action onTaskEndSuccessfully)
        {
            int retriesCount = 0;
            while (true)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    byte[] data = await _httpClient.GetByteArrayAsync(chunk.Url, token);
                    await File.WriteAllBytesAsync(chunk.Location, data, token);
                    onTaskEndSuccessfully.Invoke();
                    return;
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    if (_settingsService.Data.Twitch.Vod.ChunkDownloadingError.Retry && retriesCount < _settingsService.Data.Twitch.Vod.ChunkDownloadingError.RetriesCount)
                    {
                        retriesCount++;
                        await Task.Delay(_settingsService.Data.Twitch.Vod.ChunkDownloadingError.RetryDelay);
                    }
                    else throw;
                }
            }
        }

        #endregion
    }
}
