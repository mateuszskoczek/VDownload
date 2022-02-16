using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using VDownload.Core.Enums;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Models;
using Windows.Storage;

namespace VDownload.Core.Services.Sources.Twitch
{
    public class Clip : IVideoService
    {
        #region CONSTANTS



        #endregion



        #region CONSTRUCTORS

        public Clip(string id)
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

        // GET CLIP METADATA
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
            JToken response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/clips")).GetValue("data")[0];
            
            // Set parameters
            Title = (string)response["title"];
            Author = (string)response["broadcaster_name"];
            Date = Convert.ToDateTime(response["created_at"]);
            Duration = TimeSpan.FromSeconds((double)response["duration"]);
            Views = (long)response["view_count"];
            Thumbnail = new Uri((string)response["thumbnail_url"]);
        }

        public async Task GetStreamsAsync()
        {
            // Create client
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            client.Headers.Add("Client-ID", Auth.GQLApiClientID);

            // Get video streams
            JToken[] response = JArray.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"))[0]["data"]["clip"]["videoQualities"].ToArray();
            
            // Init streams list
            List<Stream> streams = new List<Stream>();
            
            // Parse response
            foreach (JToken streamData in response)
            {
                // Get info
                Uri url = new Uri((string)streamData["sourceURL"]);
                int height = int.Parse((string)streamData["quality"]);
                int frameRate = (int)streamData["frameRate"];

                // Create stream
                Stream stream = new Stream(url, false, StreamType.AudioVideo)
                {
                    Height = height,
                    FrameRate = frameRate
                };

                // Add stream
                streams.Add(stream);
            }
            
            // Set Streams parameter
            Streams = streams.ToArray();
        }

        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, Stream audioVideoStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default)
        {
            // Set cancellation token
            cancellationToken.ThrowIfCancellationRequested();

            // Invoke DownloadingStarted event
            DownloadingStarted?.Invoke(this, EventArgs.Empty);

            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Auth.GQLApiClientID);

            // Get video GQL access token
            JToken videoAccessToken = JArray.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"))[0]["data"]["clip"]["playbackAccessToken"];
            
            // Download
            StorageFile rawFile = await downloadingFolder.CreateFileAsync("raw.mp4");
            using (client = new WebClient())
            {
                client.DownloadProgressChanged += (s, a) => { DownloadingProgressChanged(this, new ProgressChangedEventArgs(a.ProgressPercentage, null)); };
                client.QueryString.Add("sig", (string)videoAccessToken["signature"]);
                client.QueryString.Add("token", HttpUtility.UrlEncode((string)videoAccessToken["value"]));
                using (cancellationToken.Register(client.CancelAsync))
                {
                    await client.DownloadFileTaskAsync(audioVideoStream.Url, rawFile.Path);
                }
            }
            DownloadingCompleted?.Invoke(this, EventArgs.Empty);

            // Processing
            StorageFile outputFile = rawFile;
            if (extension != MediaFileExtension.MP4 || mediaType != MediaType.AudioVideo || trimStart > new TimeSpan(0) || trimEnd < Duration)
            {
                outputFile = await downloadingFolder.CreateFileAsync($"transcoded.{extension.ToString().ToLower()}");
                MediaProcessor mediaProcessor = new MediaProcessor(outputFile, trimStart, trimEnd);
                mediaProcessor.ProcessingStarted += ProcessingStarted;
                mediaProcessor.ProcessingProgressChanged += ProcessingProgressChanged;
                mediaProcessor.ProcessingCompleted += ProcessingCompleted;
                await mediaProcessor.Run(rawFile, extension, mediaType, cancellationToken);
            }

            // Return output file
            return outputFile;
        }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, Stream audioVideoStream, MediaFileExtension extension, MediaType mediaType, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, audioVideoStream, extension, mediaType, new TimeSpan(0), Duration, cancellationToken); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, IVStream videoStream, VideoFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default) { throw new NotImplementedException("Twitch Clip download service doesn't support separate video and audio streams"); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, IVStream videoStream, VideoFileExtension extension, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, audioStream, videoStream, extension, new TimeSpan(0), Duration, cancellationToken); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, AudioFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default) { throw new NotImplementedException("Twitch Clip download service doesn't support separate video and audio streams"); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, AudioFileExtension extension, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, audioStream, extension, new TimeSpan(0), Duration, cancellationToken); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IVStream videoStream, VideoFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default) { throw new NotImplementedException("Twitch Clip download service doesn't support separate video and audio streams"); }
        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IVStream videoStream, VideoFileExtension extension, CancellationToken cancellationToken = default) { return await DownloadAndTranscodeAsync(downloadingFolder, videoStream, extension, new TimeSpan(0), Duration, cancellationToken); }

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
