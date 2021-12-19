// External
using Newtonsoft.Json.Linq;

// System
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VDownload.Services;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Sources.Twitch
{
    internal class Clip
    {
        #region INIT

        // ID
        private string ID { get; set; }

        // CONSTRUCTOR
        public Clip(string id)
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

            // Get metadata
            Uri requestUrl = new Uri($"https://api.twitch.tv/kraken/clips/{ID}");
            JObject response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUrl));

            // Pack data into dictionary
            Dictionary<string, object> metadata = new Dictionary<string, object>
            {
                ["title"] = response["title"].ToString().Replace("\n", ""),
                ["author"] = response["broadcaster"]["display_name"].ToString(),
                ["date"] = Convert.ToDateTime(response["created_at"].ToString()),
                ["duration"] = TimeSpan.FromSeconds(Math.Ceiling(double.Parse(response["duration"].ToString()))),
                ["views"] = long.Parse(response["views"].ToString()),
                ["url"] = new Uri(response["url"].ToString()),
                ["thumbnail"] = new Uri(response["thumbnails"]["medium"].ToString()),
            };

            // Return metadata
            return metadata;
        }

        // GET STREAMS
        public async Task<Dictionary<string, Uri>> GetStreams()
        {
            // Client settings
            WebClient Client = new WebClient();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            Client.Encoding = Encoding.UTF8;

            // Get streams
            var response = JArray.Parse(await Client.UploadStringTaskAsync(new Uri("https://gql.twitch.tv/gql", UriKind.Absolute), "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"));

            // Pack data into dictioanry
            Dictionary<string, Uri> streams = new Dictionary<string, Uri>();
            foreach (var s in response[0]["data"]["clip"]["videoQualities"])
            {
                string key = $"{s["quality"]}p{s["frameRate"]}";
                Uri value = new Uri(s["sourceURL"].ToString());
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

            // Download
            StorageFile rawFile = null;
            if (!token.Token.IsCancellationRequested)
            {
                try
                {
                    rawFile = await tempFolder.CreateFileAsync("raw.mp4");
                    if (!token.Token.IsCancellationRequested)
                    {
                        // Access token client settings
                        WebClient Client = new WebClient();
                        Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
                        Client.Encoding = Encoding.UTF8;

                        // Get access token
                        var response = JArray.Parse(await Client.UploadStringTaskAsync(new Uri("https://gql.twitch.tv/gql", UriKind.Absolute), "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"));
                        string tokenVal = response[0]["data"]["clip"]["playbackAccessToken"]["value"].ToString();
                        string tokenSig = response[0]["data"]["clip"]["playbackAccessToken"]["signature"].ToString();

                        // Downloading client settings
                        Client = new WebClient();
                        Client.DownloadProgressChanged += OnProgress;

                        // Download
                        string downloadUrl = $"{(await GetStreams())[quality].OriginalString}?sig={tokenSig}&token={HttpUtility.UrlEncode(tokenVal)}";
                        token.Token.ThrowIfCancellationRequested();
                        using (token.Token.Register(Client.CancelAsync))
                        {
                            await Client.DownloadFileTaskAsync(downloadUrl, rawFile.Path);
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
                if (extension != "MP4" && trimStart.TotalMilliseconds > 0 && trimEnd.TotalMilliseconds < duration.TotalMilliseconds)
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
                else
                {
                    outputFile = rawFile;
                }
            }

            // Return output file
            return outputFile;
        }

        #endregion



        #region EVENT HANDLERS

        private void OnProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressLabelTextblock.Text = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelDownloading")} ({e.ProgressPercentage}%)";
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Value = e.ProgressPercentage;
        }

        #endregion
    }
}
