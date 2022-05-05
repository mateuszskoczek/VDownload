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
using VDownload.Core.Services.Sources.Twitch.Helpers;
using VDownload.Core.Structs;
using Windows.Storage;

namespace VDownload.Core.Services.Sources.Twitch
{
    [Serializable]
    public class Clip : IVideo
    {
        #region CONSTRUCTORS

        public Clip(string id)
        {
            ID = id;
            Source = VideoSource.TwitchClip;
        }

        #endregion



        #region PROPERTIES

        public VideoSource Source { get; private set; }
        public string ID { get; private set; }
        public Uri Url { get; private set; }
        public Metadata Metadata { get; private set; }
        public BaseStream[] BaseStreams { get; private set; }

        #endregion



        #region STANDARD METHODS

        // GET CLIP METADATA
        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get response
            JToken response = null;
            using (WebClient client = await Client.Helix())
            {
                client.QueryString.Add("id", ID);
                response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/clips")).GetValue("data");
                if (((JArray)response).Count > 0) response = response[0];
                else throw new MediaNotFoundException($"Twitch Clip (ID: {ID}) was not found");
            }

            // Create unified video url
            Url = new Uri($"https://clips.twitch.tv/{ID}");

            // Set metadata
            Metadata = new Metadata()
            {
                Title = (string)response["title"],
                Author = (string)response["broadcaster_name"],
                Date = Convert.ToDateTime(response["created_at"]),
                Duration = TimeSpan.FromSeconds((double)response["duration"]),
                Views = (long)response["view_count"],
                Thumbnail = new Uri((string)response["thumbnail_url"]),
            };
        }

        public async Task GetStreamsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get response
            JToken[] response;
            using (WebClient client = Client.GQL())
            {
                response = JArray.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"))[0]["data"]["clip"]["videoQualities"].ToArray();
            }

            // Init streams list
            List<BaseStream> streams = new List<BaseStream>();
            
            // Parse response
            foreach (JToken streamData in response)
            {
                // Create stream
                BaseStream stream = new BaseStream()
                {
                    Url = new Uri((string)streamData["sourceURL"]),
                    Height = int.Parse((string)streamData["quality"]),
                    FrameRate = (int)streamData["frameRate"],
                };

                // Add stream
                streams.Add(stream);
            }
            
            // Set streams
            BaseStreams = streams.ToArray();
        }

        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, BaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default)
        {
            // Invoke DownloadingStarted event
            cancellationToken.ThrowIfCancellationRequested();
            DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

            // Get video GQL access token
            JToken videoAccessToken = null;
            using (WebClient client = Client.GQL())
            {
                videoAccessToken = JArray.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"))[0]["data"]["clip"]["playbackAccessToken"];
            }

            // Download
            cancellationToken.ThrowIfCancellationRequested();
            StorageFile rawFile = await downloadingFolder.CreateFileAsync("raw.mp4");
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += (s, a) => { DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(a.ProgressPercentage)); };
                client.QueryString.Add("sig", (string)videoAccessToken["signature"]);
                client.QueryString.Add("token", HttpUtility.UrlEncode((string)videoAccessToken["value"]));
                cancellationToken.ThrowIfCancellationRequested();
                using (cancellationToken.Register(client.CancelAsync))
                {
                    await client.DownloadFileTaskAsync(baseStream.Url, rawFile.Path);
                }
            }
            DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(100, true));

            // Processing
            StorageFile outputFile = rawFile;
            if (extension != MediaFileExtension.MP4 || mediaType != MediaType.AudioVideo || trimStart != null || trimEnd != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                outputFile = await downloadingFolder.CreateFileAsync($"transcoded.{extension.ToString().ToLower()}");

                MediaProcessor mediaProcessor = new MediaProcessor();
                mediaProcessor.ProgressChanged += ProcessingProgressChanged;

                Task mediaProcessorTask;
                if (trimStart == TimeSpan.Zero && trimEnd == Metadata.Duration) mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, cancellationToken: cancellationToken);
                else if (trimStart == TimeSpan.Zero) mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trimStart: trimStart, cancellationToken: cancellationToken);
                else if (trimEnd == Metadata.Duration) mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trimEnd: trimEnd, cancellationToken: cancellationToken);
                else mediaProcessorTask = mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trimStart, trimEnd, cancellationToken);
                await mediaProcessorTask;
            }

            // Return output file
            return outputFile;
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<EventArgs.ProgressChangedEventArgs> DownloadingProgressChanged;
        public event EventHandler<EventArgs.ProgressChangedEventArgs> ProcessingProgressChanged;

        #endregion
    }
}
