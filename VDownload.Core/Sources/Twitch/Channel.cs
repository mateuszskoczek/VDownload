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
            Source = PlaylistSource.TwitchChannel;
            ID = id;
            Url = new Uri($"https://twitch.tv/{ID}");
        }

        #endregion



        #region PROPERTIES

        public PlaylistSource Source { get; private set; }
        public string ID { get; private set; }
        private string UniqueID { get; set; }
        public Uri Url { get; private set; }
        public string Name { get; private set; }
        public IVideo[] Videos { get; private set; }

        #endregion



        #region PUBLIC METHODS

        public async Task GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            JToken response = null;
            using (WebClient client = await Client.Helix())
            {
                client.QueryString.Add("login", ID);
                response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/users"))["data"];
                if (((JArray)response).Count > 0) response = response[0];
                else throw new MediaNotFoundException($"Twitch Channel (ID: {ID}) was not found");
            }

            UniqueID = (string)response["id"];
            Name = (string)response["display_name"];
        }

        public async Task GetVideosAsync(CancellationToken cancellationToken = default) => await GetVideosAsync(0, cancellationToken);
        public async Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string pagination = "";

            List<Vod> videos = new List<Vod>();

            bool getAll = numberOfVideos == 0;
            int count;
            JToken[] videosData;
            List<Task> getStreamsTasks = new List<Task>();
            do
            {
                count = numberOfVideos < 100 && !getAll ? numberOfVideos : 100;

                JToken response = null;
                using (WebClient client = await Client.Helix())
                {
                    client.QueryString.Add("user_id", UniqueID);
                    client.QueryString.Add("first", count.ToString());
                    client.QueryString.Add("after", pagination);
                    response = JObject.Parse(await client.DownloadStringTaskAsync("https://api.twitch.tv/helix/videos"));
                }

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
            while ((getAll || numberOfVideos > 0) && count == videosData.Length);

            await Task.WhenAll(getStreamsTasks);

            Videos = videos.ToArray();
        }

        #endregion
    }
}
