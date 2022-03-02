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
using VDownload.Core.Objects;
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



        #region PROPERTIES

        public string ID { get; private set; }
        public Uri VideoUrl { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public DateTime Date { get; private set; }
        public TimeSpan Duration { get; private set; }
        public long Views { get; private set; }
        public Uri Thumbnail { get; private set; }
        public IBaseStream[] BaseStreams { get; private set; }

        #endregion



        #region STANDARD METHODS

        // GET CLIP METADATA
        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            // Get access token
            cancellationToken.ThrowIfCancellationRequested();
            string accessToken = await Auth.ReadAccessTokenAsync();
            if (accessToken == null) throw new TwitchAccessTokenNotFoundException();

            // Check access token
            cancellationToken.ThrowIfCancellationRequested();
            var twitchAccessTokenValidation = await Auth.ValidateAccessTokenAsync(accessToken);
            if (!twitchAccessTokenValidation.IsValid) throw new TwitchAccessTokenNotValidException();

            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Bearer {accessToken}");
            client.Headers.Add("Client-Id", Auth.ClientID);

            // Get response
            client.QueryString.Add("id", ID);
            JToken response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/clips")).GetValue("data")[0];

            // Create unified video url
            VideoUrl = new Uri($"https://clips.twitch.tv/{ID}");

            // Set parameters
            Title = (string)response["title"];
            Author = (string)response["broadcaster_name"];
            Date = Convert.ToDateTime(response["created_at"]);
            Duration = TimeSpan.FromSeconds((double)response["duration"]);
            Views = (long)response["view_count"];
            Thumbnail = new Uri((string)response["thumbnail_url"]);
        }

        public async Task GetStreamsAsync(CancellationToken cancellationToken = default)
        {
            // Create client
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            client.Headers.Add("Client-ID", Auth.GQLApiClientID);

            // Get video streams
            cancellationToken.ThrowIfCancellationRequested();
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
            BaseStreams = streams.ToArray();
        }

        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IBaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default)
        {
            // Invoke DownloadingStarted event
            DownloadingStarted?.Invoke(this, System.EventArgs.Empty);

            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Auth.GQLApiClientID);

            // Get video GQL access token
            cancellationToken.ThrowIfCancellationRequested();
            JToken videoAccessToken = JArray.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"))[0]["data"]["clip"]["playbackAccessToken"];

            // Download
            cancellationToken.ThrowIfCancellationRequested();
            StorageFile rawFile = await downloadingFolder.CreateFileAsync("raw.mp4");
            using (client = new WebClient())
            {
                client.DownloadProgressChanged += (s, a) => { DownloadingProgressChanged(this, new ProgressChangedEventArgs(a.ProgressPercentage, null)); };
                client.QueryString.Add("sig", (string)videoAccessToken["signature"]);
                client.QueryString.Add("token", HttpUtility.UrlEncode((string)videoAccessToken["value"]));
                cancellationToken.ThrowIfCancellationRequested();
                using (cancellationToken.Register(client.CancelAsync))
                {
                    await client.DownloadFileTaskAsync(baseStream.Url, rawFile.Path);
                }
            }
            DownloadingCompleted?.Invoke(this, System.EventArgs.Empty);

            // Processing
            StorageFile outputFile = rawFile;
            if (extension != MediaFileExtension.MP4 || mediaType != MediaType.AudioVideo || trimStart > new TimeSpan(0) || trimEnd < Duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                outputFile = await downloadingFolder.CreateFileAsync($"transcoded.{extension.ToString().ToLower()}");
                MediaProcessor mediaProcessor = new MediaProcessor(outputFile, trimStart, trimEnd);
                mediaProcessor.ProcessingStarted += ProcessingStarted;
                mediaProcessor.ProcessingProgressChanged += ProcessingProgressChanged;
                mediaProcessor.ProcessingCompleted += ProcessingCompleted;
                cancellationToken.ThrowIfCancellationRequested();
                await mediaProcessor.Run(rawFile, extension, mediaType, cancellationToken);
            }

            // Return output file
            return outputFile;
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
