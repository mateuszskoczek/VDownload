using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services.Sources.Twitch.Helpers;

namespace VDownload.Core.Services.Sources.Twitch
{
    [Serializable]
    public class Channel : IPlaylist
    {
        #region CONSTRUCTORS

        public Channel(string id)
        {
            ID = id;
            Source = PlaylistSource.TwitchChannel;
        }

        #endregion



        #region PROPERTIES

        public string ID { get; private set; }
        public PlaylistSource Source { get; private set; }
        public Uri Url { get; private set; }
        public string Name { get; private set; }
        public IVideo[] Videos { get; private set; }
        private string UniqueUserID { get; set; }

        #endregion



        #region STANDARD METHODS

        // GET CHANNEL METADATA
        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get response
            JToken response = null;
            using (WebClient client = await Client.Helix())
            {
                client.QueryString.Add("login", ID);
                response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/users"))["data"];
                if (((JArray)response).Count > 0) response = response[0];
                else throw new MediaNotFoundException($"Twitch Channel (ID: {ID}) was not found");
            }

            // Create unified playlist url
            Url = new Uri($"https://twitch.tv/{ID}");

            // Set parameters
            UniqueUserID = (string)response["id"];
            Name = (string)response["display_name"];
        }

        // GET CHANNEL VIDEOS
        public async Task GetVideosAsync(CancellationToken cancellationToken = default) => await GetVideosAsync(0, cancellationToken);
        public async Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Set page id
            string pagination = "";

            // Set array of videos
            List<Vod> videos = new List<Vod>();

            // Get all
            bool getAll = numberOfVideos == 0;

            // Get videos
            int count;
            JToken[] videosData;
            List<Task> getStreamsTasks = new List<Task>();
            do
            {
                // Set number of videos to get in this iteration
                count = numberOfVideos < 100 && !getAll ? numberOfVideos : 100;

                // Get response
                JToken response = null;
                using (WebClient client = await Client.Helix())
                {
                    client.QueryString.Add("user_id", UniqueUserID);
                    client.QueryString.Add("first", count.ToString());
                    client.QueryString.Add("after", pagination);
                    response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/videos"));
                }

                // Set page id
                pagination = (string)response["pagination"]["cursor"];

                // Set videos data
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
            while ((getAll || numberOfVideos > 0) && count == videosData.Length);

            // Wait for all getStreams tasks
            await Task.WhenAll(getStreamsTasks);

            // Set videos
            Videos = videos.ToArray();
        }

        #endregion
    }
}
