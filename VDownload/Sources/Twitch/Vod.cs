// Internal
using VDownload.Services;

// External
using Newtonsoft.Json.Linq;

// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Sources.Twitch
{
    internal class Vod
    {
        #region INIT

        // ID
        private string ID { get; set; }

        // CONSTRUCTOR
        public Vod(string id)
        {
            ID = id;
        }

        #endregion



        #region MAIN

        // GET METADATA
        public async Task<Dictionary<string, object>> GetMetadata()
        {
            // Client settings
            WebClient Client = new WebClient();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

            // Send request and get response
            Uri requestUrl = new Uri($"https://api.twitch.tv/kraken/videos/{ID}");
            JObject response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUrl));

            // Pack data into dictionary
            Dictionary<string, object> metadata = new Dictionary<string, object>()
            {
                ["title"] = response["title"].ToString().Replace("\n", ""),
                ["author"] = response["channel"]["display_name"].ToString(),
                ["date"] = Convert.ToDateTime(response["created_at"].ToString()),
                ["duration"] = TimeSpan.FromSeconds(int.Parse(response["length"].ToString())),
                ["views"] = long.Parse(response["views"].ToString()),
                ["url"] = new Uri(response["url"].ToString())
            };
            try
            { metadata["thumbnail"] = new Uri(response["thumbnails"]["large"][0]["url"].ToString()); }
            catch
            { metadata["thumbnail"] = new Uri("Assets/Icons/Unknown/Thumbnail.png", UriKind.Relative); }

            // Return metadata
            return metadata;
        }

        // GET STREAMS
        public async Task<Dictionary<string, Uri>> GetStreams()
        {
            // Client settings
            WebClient Client = new WebClient();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");

            // Get access token
            JObject accessToken = JObject.Parse(await Client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + ID + "\",\"playerType\":\"embed\"}}"));
            string tokenVal = accessToken["data"]["videoPlaybackAccessToken"]["value"].ToString();
            string tokenSig = accessToken["data"]["videoPlaybackAccessToken"]["signature"].ToString();

            // Get streams
            string[] response = Client.DownloadString($"http://usher.twitch.tv/vod/{ID}?nauth={tokenVal}&nauthsig={tokenSig}&allow_source=true&player=twitchweb").Split("\n");

            // Pack data into dictionary
            Dictionary<string, Uri> streams = new Dictionary<string, Uri>();
            for (int i = 2; i < response.Length; i += 3)
            {
                string key = response[i].Replace("#EXT-X-MEDIA:", "").Split(',')[2].Replace("NAME=", "").Replace("\"", "");
                Uri value = new Uri(response[i + 2]);
                streams[key] = value;
            }

            // Return streams
            return streams;
        }

        // DOWNLOAD VIDEO
        private TextBlock ProgressLabelTextblock;
        private ProgressBar ProgressBar;
        private Image ProgressIcon;
        public async Task<StorageFile> Download(StorageFolder tempFolder, string quality, string extension, string mediaType, TimeSpan trimStart, TimeSpan trimEnd, TimeSpan duration, CancellationTokenSource token, TextBlock progressLabelTextblock, ProgressBar progressBar, Image progressIcon)
        {
            // Set variables
            ProgressLabelTextblock = progressLabelTextblock;
            ProgressBar = progressBar;
            ProgressIcon = progressIcon;

            // Set progress to downloading
            if (!token.Token.IsCancellationRequested)
            {
                ProgressLabelTextblock.Text = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelDownloading")} (0%)";
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                ProgressIcon.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Downloading.png") };
            }

            // Get chunks
            Uri[] chunks = new Uri[] { };
            float chunksInTotal = 0;
            float chunksDownloaded = 0;
            if (!token.Token.IsCancellationRequested)
            {
                chunks = await ExtractChunksFromM3U8((await GetStreams())[quality]);
                chunksInTotal = chunks.Length;
            }

            // Download
            StorageFile rawFile = null;
            if (!token.Token.IsCancellationRequested)
            {
                try
                {
                    rawFile = await tempFolder.CreateFileAsync("raw.ts");
                    if (!token.Token.IsCancellationRequested)
                    {
                        Task writeTask = WriteChunkToFile(rawFile, await DownloadVodChunk(chunks[0]));
                        chunksDownloaded++;
                        ProgressLabelTextblock.Text = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelDownloading")} ({Math.Floor(chunksDownloaded / chunksInTotal * 100)}%)";
                        ProgressBar.IsIndeterminate = false;
                        ProgressBar.Visibility = Visibility.Visible;
                        ProgressBar.Value = chunksDownloaded / chunksInTotal * 100;
                        Task<byte[]> downloadTask = DownloadVodChunk(chunks[1]);
                        for (int i = 2; i < chunks.Length; i++)
                        {
                            if (!token.Token.IsCancellationRequested)
                            {
                                await Task.WhenAll(downloadTask, writeTask);
                                chunksDownloaded++;
                                ProgressLabelTextblock.Text = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelDownloading")} ({Math.Floor(chunksDownloaded / chunksInTotal * 100)}%)";
                                ProgressBar.IsIndeterminate = false;
                                ProgressBar.Visibility = Visibility.Visible;
                                ProgressBar.Value = chunksDownloaded / chunksInTotal * 100;
                                writeTask = WriteChunkToFile(rawFile, downloadTask.Result);
                                downloadTask = DownloadVodChunk(chunks[i]);
                            }
                        }
                    }
                }
                catch (WebException)
                {
                    throw new Exception(ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelErrorInternetConnection"));
                }
            }

            // Rendering
            StorageFile outputFile = null;
            if (!token.Token.IsCancellationRequested)
            {
                // Set progress to transcoding
                ProgressLabelTextblock.Text = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelTranscoding")} (0%)";
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.Value = 0;
                ProgressIcon.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Transcoding.png") };

                // Create transcoding output file
                outputFile = await tempFolder.CreateFileAsync($"processed.{extension.ToLower()}");
                
                // Run processing
                await new Media().Transcode(rawFile, outputFile, extension, mediaType, duration, trimStart, trimEnd, token.Token, ProgressLabelTextblock, ProgressBar);
            }

            // Return output file
            return outputFile;
        }

        #endregion



        #region INTERNAL

        // EXTRACT CHUNKS FROM M3U8 PLAYLIST
        private async Task<Uri[]> ExtractChunksFromM3U8(Uri streamUrl)
        {
            // Client settings
            WebClient Client = new WebClient();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");

            // Get playlist content
            string request = await Client.DownloadStringTaskAsync(streamUrl);

            // Pack into list
            List<Uri> videos = new List<Uri>();
            foreach (string l in request.Split("\n"))
            {
                if (l.Length > 0 && l[0] != '#')
                {
                    string[] uriSegments = streamUrl.Segments;
                    string streamDir = "";
                    for (int i = 0; i < uriSegments.Length - 1; i++)
                    {
                        streamDir += uriSegments[i];
                    }
                    videos.Add(new Uri($@"{streamUrl.GetLeftPart(UriPartial.Scheme)}{streamUrl.Host}{streamDir}{l}"));
                }
            }

            // Return videos
            return videos.ToArray();
        }

        // DOWNLOAD VOD CHUNK
        private async Task<byte[]> DownloadVodChunk(Uri url)
        {
            // Download
            int errorCount = 0;
            bool done = false;
            while (!done && errorCount < 10)
            {
                try
                {
                    using (WebClient Client = new WebClient())
                    {
                        return await Client.DownloadDataTaskAsync(url);
                    }
                }
                catch
                {
                    errorCount++;
                    await Task.Delay(5000);
                }
            }
            throw new WebException();
        }

        // WRITE CHUNK TO FILE
        private Task WriteChunkToFile(StorageFile file, byte[] dataToWrite)
        {
            return Task.Factory.StartNew(() =>
            {
                using (var stream = new FileStream(file.Path, FileMode.Append))
                {
                    stream.Write(dataToWrite, 0, dataToWrite.Length);
                    stream.Close();
                }
            });
        }

        #endregion
    }
}
