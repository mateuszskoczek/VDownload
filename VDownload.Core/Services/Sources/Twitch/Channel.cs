using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;

namespace VDownload.Core.Services.Sources.Twitch
{
    public class Channel : IPlaylistService
    {
        #region CONSTRUCTORS

        public Channel(string id)
        {
            ID = id;
        }

        #endregion



        #region PROPERTIES

        public string ID { get; private set; }
        public string Name { get; private set; }
        public Vod[] Videos { get; private set; }

        #endregion



        #region STANDARD METHODS

        // GET CHANNEL METADATA
        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            // Set cancellation token
            cancellationToken.ThrowIfCancellationRequested();

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
            client.QueryString.Add("login", ID);
            JToken response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/users"))["data"][0];

            // Set parameters
            if (!ID.All(char.IsDigit)) ID = (string)response["id"];
            Name = (string)response["display_name"];
        }

        // GET CHANNEL VIDEOS
        public async Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default)
        {
            // Set cancellation token
            cancellationToken.ThrowIfCancellationRequested();

            // Get access token
            string accessToken = await Auth.ReadAccessTokenAsync();
            if (accessToken == null) throw new TwitchAccessTokenNotFoundException();

            // Set pagination
            string pagination = "";

            // Set array of videos
            List<Vod> videos = new List<Vod>();

            // Get videos
            int count;
            JToken[] videosData;
            List<Task> getStreamsTasks = new List<Task>();
            do
            {
                // Check access token
                var twitchAccessTokenValidation = await Auth.ValidateAccessTokenAsync(accessToken);
                if (!twitchAccessTokenValidation.IsValid) throw new TwitchAccessTokenNotValidException();

                // Create client
                WebClient client = new WebClient();
                client.Headers.Add("Authorization", $"Bearer {accessToken}");
                client.Headers.Add("Client-Id", Auth.ClientID);

                // Set number of videos to get in this iteration
                count = numberOfVideos < 100 ? numberOfVideos : 100;

                // Get response
                client.QueryString.Add("user_id", ID);
                client.QueryString.Add("first", count.ToString());
                client.QueryString.Add("after", pagination);

                JToken response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/videos"));

                pagination = (string)response["pagination"]["cursor"];
                videosData = response["data"].ToArray();

                foreach (JToken videoData in videosData)
                {
                    Vod video = new Vod((string)videoData["id"]);
                    video.GetMetadataAsync(videoData);
                    getStreamsTasks.Add(video.GetStreamsAsync());
                    videos.Add(video);

                    numberOfVideos--;
                }
            }
            while (numberOfVideos > 0 && count == videosData.Length);

            // Wait for all getStreams tasks
            await Task.WhenAll(getStreamsTasks);

            // Set Videos parameter
            Videos = videos.ToArray();
        }

        #endregion
    }
}
