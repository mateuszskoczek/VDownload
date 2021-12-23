using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services;

namespace VDownload.Sources.Twitch
{
    internal class Channel
    {
        // GET VIDEOS
        public static async Task<VObject[]> GetVideos(string ID)
        {
            // Client settings
            WebClient Client = new WebClient();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

            
            // Get channel id
            Uri requestUri;
            JObject response;
            if (!ID.All(char.IsDigit))
            {
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
            List<VObject> videos = new List<VObject>();
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
                    if (int.Parse(Config.GetValue("max_playlist_videos")) == 0 || videos.Count < int.Parse(Config.GetValue("max_playlist_videos")))
                    {
                        try
                        {
                            VObject vid = new VObject(new Uri($"https://www.twitch.tv/videos/{v["_id"].ToString().Replace("v", "")}"));
                            await vid.GetMetadata();
                            videos.Add(vid);
                        }
                        catch { }
                    }
                    else
                    {
                        break;
                    }
                }

                if (response["videos"].ToArray().Length == 100 && !(int.Parse(Config.GetValue("max_playlist_videos")) == 0 || videos.Count < int.Parse(Config.GetValue("max_playlist_videos"))))
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
    }
}
