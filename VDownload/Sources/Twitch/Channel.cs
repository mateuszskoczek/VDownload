using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch
{
    internal class Channel
    {
        #region INIT

        // ID
        private string ID { get; set; }

        // CONSTRUCTOR
        public Channel(string id)
        {
            ID = id;
        }

        #endregion



        #region MAIN

        // GET VIDEOS
        public async Task<string[]> GetVideos()
        {
            // Client settings
            WebClient Client = new WebClient();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "uo6dggojyb8d6soh92zknwmi5ej1q2");

            // Get channel id
            Uri requestUri;
            JObject response;
            if (!ID.All(char.IsDigit))
            {
                Debug.WriteLine(ID);
                requestUri = new Uri($"https://api.twitch.tv/kraken/users?login={ID}");
                response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));
                response = (JObject)response["users"][0];
            }
            else
            {
                requestUri = new Uri($"https://api.twitch.tv/kraken/users/{ID}");
                response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));
            }
            string id = response["_id"].ToString();

            // Get list
            List<string> videos = new List<string>();
            int offset = 0;
            do
            {
                Client = new WebClient();
                Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
                Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

                requestUri = new Uri($"https://api.twitch.tv/kraken/channels/{id}/videos?limit=100&offset={offset}");
                response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));
                foreach (var v in response["videos"])
                {
                    Debug.WriteLine(v["_id"].ToString().Replace("v", ""));
                    videos.Add(v["_id"].ToString().Replace("v", ""));
                }

                if (response["videos"].ToArray().Length == 100)
                {
                    offset += 100;
                }
                else
                {
                    break;
                }
            } while (true);

            // Return videos
            return videos.ToArray();
        }

        #endregion
    }
}
