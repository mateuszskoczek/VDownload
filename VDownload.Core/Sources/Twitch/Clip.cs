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
            Source = VideoSource.TwitchClip;
            ID = id;
            Url = new Uri($"https://clips.twitch.tv/{ID}");
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
                response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/clips")).GetValue("data");
                if (((JArray)response).Count > 0) response = response[0];
                else throw new MediaNotFoundException($"Twitch Clip (ID: {ID}) was not found");
            }

            Title = (string)response["title"];
            Author = (string)response["broadcaster_name"];
            Date = Convert.ToDateTime(response["created_at"]);
            Duration = TimeSpan.FromSeconds((double)response["duration"]);
            Views = (long)response["view_count"];
            Thumbnail = new Uri((string)response["thumbnail_url"]);
        }

        public async Task GetStreamsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            JToken[] response;
            using (WebClient client = Client.GQL())
            {
                response = JArray.Parse(await client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + ID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"))[0]["data"]["clip"]["videoQualities"].ToArray();
            }

            List<BaseStream> streams = new List<BaseStream>();
            
            foreach (JToken streamData in response)
            {
                BaseStream stream = new BaseStream()
                {
                    Url = new Uri((string)streamData["sourceURL"]),
                    Height = int.Parse((string)streamData["quality"]),
                    FrameRate = (int)streamData["frameRate"],
                };

                streams.Add(stream);
            }
            
            BaseStreams = streams.ToArray();
        }

        public async Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, BaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TrimData trim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            DownloadingProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

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
            if (extension != MediaFileExtension.MP4 || mediaType != MediaType.AudioVideo || trim.Start != null || trim.End != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                outputFile = await downloadingFolder.CreateFileAsync($"transcoded.{extension.ToString().ToLower()}");

                MediaProcessor mediaProcessor = new MediaProcessor();
                mediaProcessor.ProgressChanged += ProcessingProgressChanged;

                await mediaProcessor.Run(rawFile, extension, mediaType, outputFile, trim, cancellationToken);
            }

            return outputFile;
        }

        #endregion



        #region EVENTS

        public event EventHandler<EventArgs.ProgressChangedEventArgs> DownloadingProgressChanged;
        public event EventHandler<EventArgs.ProgressChangedEventArgs> ProcessingProgressChanged;

        #endregion
    }
}
