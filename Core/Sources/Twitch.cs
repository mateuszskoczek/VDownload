// System
using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

// External
using Newtonsoft.Json.Linq;
using Dasync.Collections;

// Internal
using VDownload.Core.Services;
using System.Threading;

namespace VDownload.Core.Sources
{
    class Twitch
    {
        #region VOD

        // GET VOD METADATA
        public static async Task<Dictionary<string, string>> GetVodMetadata(string vodID)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

            // Get metadata
            Uri requestUri = new($"https://api.twitch.tv/kraken/videos/{vodID}");
            var response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));

            // Pack into dictionary
            Dictionary<string, string> metadata = new();
            metadata["title"] = response["title"].ToString().Replace("\n", "");
            metadata["author_name"] = response["channel"]["display_name"].ToString();
            metadata["author_id"] = response["channel"]["_id"].ToString();
            metadata["date"] = Convert.ToDateTime(response["created_at"].ToString()).ToString(Config.Read("date_format"));
            metadata["duration"] = TimeSpan.FromSeconds(int.Parse(response["length"].ToString())).ToString();
            metadata["views"] = response["views"].ToString();
            try
            {
                metadata["thumbnail"] = response["thumbnails"]["large"][0]["url"].ToString();
            }
            catch
            {
                metadata["thumbnail"] = "Unavailable";
            }
            metadata["url"] = response["url"].ToString();
            metadata["id"] = response["_id"].ToString().Trim('v');
            
            // Return metadata
            return metadata;
        }


        // GET VOD STREAMS
        public static async Task<Dictionary<int, Dictionary<string, string>>> GetVodStreams(string vodID)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");

            // Get access token
            var accessToken = JObject.Parse(await Client.UploadStringTaskAsync("https://gql.twitch.tv/gql", "{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + vodID + "\",\"playerType\":\"embed\"}}"));
            string tokenVal = accessToken["data"]["videoPlaybackAccessToken"]["value"].ToString();
            string tokenSig = accessToken["data"]["videoPlaybackAccessToken"]["signature"].ToString();

            // Get streams
            string[] response = (await Client.DownloadStringTaskAsync($"http://usher.twitch.tv/vod/{vodID}?nauth={tokenVal}&nauthsig={tokenSig}&allow_source=true&player=twitchweb")).Split("\n")[2..];

            // Pack into dictionary
            Dictionary<int, Dictionary<string, string>> streams = new();
            int streamIndex = 0;
            for (int i = 0; i < response.Length; i += 3)
            {
                string[] line1 = response[i].Replace("#EXT-X-MEDIA:", "").Split(',');
                string[] line2 = response[i + 1].Replace("#EXT-X-STREAM-INF:", "").Split(',');
                string streamUrl = response[i + 2];
                streams[streamIndex] = new()
                {
                    { "quality", line1[2].Replace("NAME=", "").Trim('"').Split('p')[0] },
                    { "fps", line2[5].Replace("FRAME-RATE=", "") },
                    { "video_codec", line2[1].Replace("CODECS=", "").Trim('"') },
                    { "audio_codec", line2[2].Trim('"') },
                    { "url", streamUrl }
                };
                streamIndex++;
            }

            // Return streams
            return streams;
        }


        // DOWNLOAD VOD CHUNK
        public static async Task<byte[]> DownloadVodChunk(string url)
        {
            // Download
            int errorCount = 0;
            bool done = false;
            while (!done && errorCount < 10)
            {
                try
                {
                    using WebClient Client = new();
                    return await Client.DownloadDataTaskAsync(url);
                }
                catch
                {
                    errorCount++;
                    await Task.Delay(10000);
                }
            }
            throw new WebException();
        }


        // EXTRACT VIDEOS FROM M3U8
        public static async Task<string[]> GetVodStreamChunks(string url)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");

            // Get playlist content
            string request = await Client.DownloadStringTaskAsync(url);

            // Pack into list
            List<string> videos = new();
            foreach (string l in request.Split("\n"))
            {
                if (l.Length > 0 && l[0] != '#')
                {
                    videos.Add($@"{String.Join('/', url.Split('/')[..^1])}/{l}");
                }
            }

            // Return videos
            return videos.ToArray();
        }

        #endregion



        #region CLIP

        // GET CLIP METADATA
        public static async Task<Dictionary<string, string>> GetClipMetadata(string clipID)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

            // Get metadata
            Uri requestUri = new($"https://api.twitch.tv/kraken/clips/{clipID}");
            var response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));

            // Pack into dictionary
            Dictionary<string, string> metadata = new();
            metadata["title"] = response["title"].ToString().Replace("\n", "");
            metadata["author_name"] = response["curator"]["display_name"].ToString();
            metadata["author_id"] = response["curator"]["id"].ToString();
            metadata["broadcaster_name"] = response["broadcaster"]["display_name"].ToString();
            metadata["broadcaster_id"] = response["broadcaster"]["id"].ToString();
            metadata["date"] = Convert.ToDateTime(response["created_at"].ToString()).ToString(Config.Read("date_format"));
            metadata["duration"] = TimeSpan.FromSeconds(double.Parse(response["duration"].ToString())).ToString();
            metadata["views"] = response["views"].ToString();
            metadata["thumbnail"] = response["thumbnails"]["medium"].ToString();
            metadata["url"] = response["url"].ToString();
            metadata["id"] = clipID;

            // Return metadata
            return metadata;
        }


        // GET CLIP STREAMS
        public static async Task<Dictionary<int, Dictionary<string, string>>> GetClipStreams(string clipID)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            Client.Encoding = Encoding.UTF8;

            // Get streams
            var response = JArray.Parse(await Client.UploadStringTaskAsync(new Uri("https://gql.twitch.tv/gql", UriKind.Absolute), "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + clipID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"));

            // Pack into dictionary
            Dictionary<int, Dictionary<string, string>> streams = new();
            int streamIndex = 0;
            foreach (var s in response[0]["data"]["clip"]["videoQualities"])
            {
                streams[streamIndex] = new()
                {
                    { "quality", s["quality"].ToString() },
                    { "fps", s["frameRate"].ToString() },
                    { "url", s["sourceURL"].ToString() }
                };
                streamIndex++;
            }

            // Return streams
            return streams;
        }


        // DOWNLOAD CLIP STREAM
        public static async Task DownloadClipStream(string clipID, int streamID, string outputPath, DownloadProgressChangedEventHandler eventHandler)
        {
            // Client settings
            WebClient Client = new();
            Client.DownloadProgressChanged += eventHandler;

            // Get access token
            var data = await GetClipAccessToken(clipID);

            // Download file
            string url = (await GetClipStreams(clipID))[streamID]["url"];
            url += $"?sig={data.Item2}";
            url += $"&token={HttpUtility.UrlEncode(data.Item1)}";
            await Client.DownloadFileTaskAsync(url, outputPath);
        }

        #endregion



        #region CHANNEL

        // GET CHANNEL METADATA
        public static async Task<Dictionary<string, string>> GetChannelMetadata(string channelID)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

            // Get metadata
            Uri requestUri;
            JObject response;
            if (!channelID.All(char.IsDigit))
            {
                requestUri = new($"https://api.twitch.tv/kraken/users?login={channelID}");
                response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));
                response = (JObject)response["users"][0];
            }
            else
            {
                requestUri = new($"https://api.twitch.tv/kraken/users/{channelID}");
                response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));
            }
            
            // Pack into dictionary
            Dictionary<string, string> metadata = new();
            metadata["name"] = response["display_name"].ToString();
            metadata["id"] = response["_id"].ToString();
            metadata["description"] = response["bio"].ToString();
            metadata["created_date"] = Convert.ToDateTime(response["created_at"].ToString()).ToString(Config.Read("date_format"));
            metadata["update_date"] = Convert.ToDateTime(response["updated_at"].ToString()).ToString(Config.Read("date_format"));
            metadata["logo"] = response["logo"].ToString();

            // Return metadata
            return metadata;
        }


        // GET CHANNEL VIDEOS
        public static async Task<Dictionary<int, string>> GetChannelVideos(string channelID)
        {
            channelID = (await GetChannelMetadata(channelID))["id"];

            // Client settings
            WebClient Client;

            // Get list
            Dictionary<int, string> videos = new();
            Uri requestUri;
            JObject response;
            int offset = 0;
            int index = 0;
            do
            {
                Client = new();
                Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
                Client.Headers.Add("Client-ID", "v8kfhyc2980it9e7t5hhc7baukzuj2");

                requestUri = new($"https://api.twitch.tv/kraken/channels/{channelID}/videos?limit=100&offset={offset}");
                response = JObject.Parse(await Client.DownloadStringTaskAsync(requestUri));
                foreach (var v in response["videos"])
                {
                    videos[index] = v["_id"].ToString().Replace("v", "");
                    index++;
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
            return videos;
        }

        #endregion



        #region OTHER

        public static int SelectBestStream(Dictionary<int, Dictionary<string, string>> streams)
        {
            // Max variables
            int maxQuality = 0;
            double maxFPS = 0;
            int bestID = 0;

            // Check each stream
            foreach (var s in streams)
            {
                int id = s.Key;
                int quality = int.Parse(s.Value["quality"]);
                double fps = double.Parse(s.Value["fps"].Replace('.', ','));
                if (quality > maxQuality)
                {
                    maxQuality = quality;
                    maxFPS = fps;
                    bestID = id;
                }
                else if (quality == maxQuality)
                {
                    if (fps > maxFPS)
                    {
                        maxQuality = quality;
                        maxFPS = fps;
                        bestID = id;
                    }
                }
            }

            // Return id
            return bestID;
        }

        #endregion





        #region INTERNAL

        // GET CLIP ACCESS TOKEN
        private static async Task<(string, string)> GetClipAccessToken(string clipID)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");
            Client.Encoding = Encoding.UTF8;

            // Get token
            var response = JArray.Parse(await Client.UploadStringTaskAsync(new Uri("https://gql.twitch.tv/gql", UriKind.Absolute), "[{\"operationName\":\"VideoAccessToken_Clip\",\"variables\":{\"slug\":\"" + clipID + "\"},\"extensions\":{\"persistedQuery\":{\"version\":1,\"sha256Hash\":\"36b89d2507fce29e5ca551df756d27c1cfe079e2609642b4390aa4c35796eb11\"}}}]"));
            string tokenVal = response[0]["data"]["clip"]["playbackAccessToken"]["value"].ToString();
            string tokenSig = response[0]["data"]["clip"]["playbackAccessToken"]["signature"].ToString();

            // Return token
            return (tokenVal, tokenSig);
        }

        #endregion
    }
}
