using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ConsoleTableExt;
using VDownload.Services;

namespace VDownload
{
    class Twitch
    {
        // VARIABLES
        private static readonly string clientID = "kimne78kx3ncx6brgo4mv6wki5h1ko";




        // INFORMATIONS ABOUT VOD (METADATA AND STREAMS)
        public static void VodInfo(string url)
        {
            string output;
            try
            {
                // Get metadata
                var metadata = GetVodMetadata(url);

                // Get streams
                var streams = GetStreams(url);
                List<List<object>> streamsTableData = new();
                foreach (int i in streams.Keys)
                {
                    streamsTableData.Add(new()
                    {
                        i.ToString(),
                        streams[i]["quality"],
                        streams[i]["video_codec"],
                        streams[i]["audio_codec"],
                    });
                }
                string streamsTable = ConsoleTableBuilder
                    .From(streamsTableData)
                    .WithColumn("ID", "Quality", "Video codec", "Audio codec")
                    .WithCharMapDefinition(CharMapDefinition.FramePipDefinition, Global.TableAppearance.STREAMS)
                    .Export().ToString().Trim();

                // Output
                output = TerminalOutput.Get(@"output\sources\twitch\info\vod\info.out",
                    args: new()
                    {
                        metadata["title"],
                        metadata["url"],
                        metadata["author"],
                        metadata["views"],
                        metadata["date"],
                        metadata["duration"],
                        metadata["thumbnail"],
                        streamsTable,
                    }
                );
            }
            catch (HttpRequestException)
            {
                output = TerminalOutput.Get(@"output\sources\twitch\info\vod\error\no_internet_connection.out");
            }
            catch (WebException e) 
            {
                output = TerminalOutput.Get(@"output\sources\twitch\info\vod\error\web_unknown.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\sources\twitch\info\vod\error\unknown.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            Console.WriteLine(output);
        }




        // DOWNLOAD VOD
        public static void VodDownload(string url, Dictionary<string, string> options)
        {
            
        }






        // DOWNLOAD STREAM
        private static void DownloadStream()
        {

        }



        // GET VOD METADATA
        private static Dictionary<string, string> GetVodMetadata(string url)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", clientID);

            // Get metadata
            var vod = JObject.Parse(Client.DownloadString(String.Format("https://api.twitch.tv/kraken/videos/{0}", GetIDFromUrl(url))));

            Dictionary<string, string> metadata = new();
            metadata["title"] = vod["title"].ToString().Replace("\n", "");
            metadata["author"] = vod["channel"]["display_name"].ToString();
            metadata["date"] = Convert.ToDateTime(vod["created_at"].ToString()).ToString(Config.Main.ReadKey("date_format"));
            metadata["duration"] = TimeSpan.FromSeconds(int.Parse(vod["length"].ToString())).ToString();
            metadata["views"] = vod["views"].ToString();
            metadata["thumbnail"] = vod["thumbnails"]["large"][0]["url"].ToString();
            metadata["url"] = vod["url"].ToString();
            metadata["id"] = vod["_id"].ToString().Trim('v');

            return metadata;
        }




        // GET STREAMS
        private static Dictionary<int, Dictionary<string, string>> GetStreams(string url)
        {
            // Client settings
            WebClient Client = new();
            Client.Headers.Add("Client-ID", clientID);

            // Get streams
            string id = GetIDFromUrl(url);
            string response = Client.UploadString("https://gql.twitch.tv/gql", "{\"operationName\":\"PlaybackAccessToken_Template\",\"query\":\"query PlaybackAccessToken_Template($login: String!, $isLive: Boolean!, $vodID: ID!, $isVod: Boolean!, $playerType: String!) {  streamPlaybackAccessToken(channelName: $login, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isLive) {    value    signature    __typename  }  videoPlaybackAccessToken(id: $vodID, params: {platform: \\\"web\\\", playerBackend: \\\"mediaplayer\\\", playerType: $playerType}) @include(if: $isVod) {    value    signature    __typename  }}\",\"variables\":{\"isLive\":false,\"login\":\"\",\"isVod\":true,\"vodID\":\"" + id + "\",\"playerType\":\"embed\"}}");
            var accessToken = JObject.Parse(response);
            string tokenVal = accessToken["data"]["videoPlaybackAccessToken"]["value"].ToString();
            string tokenSig = accessToken["data"]["videoPlaybackAccessToken"]["signature"].ToString();
            string[] playlist = Client.DownloadString(String.Format("http://usher.twitch.tv/vod/{0}?nauth={1}&nauthsig={2}&allow_source=true&player=twitchweb", id, tokenVal, tokenSig)).Split("\n")[2..];
            
            Dictionary<int, Dictionary<string, string>> streams = new();
            int streamIndex = 0;
            for (int i = 0; i < playlist.Length; i += 3)
            {
                string[] line1 = playlist[i].Replace("#EXT-X-MEDIA:", "").Split(',');
                string[] line2 = playlist[i + 1].Replace("#EXT-X-STREAM-INF:", "").Split(',');
                string streamUrl = playlist[i + 2];
                streams[streamIndex] = new()
                {
                    { "quality", line1[2].Replace("NAME=", "").Trim('"') },
                    { "video_codec", line2[1].Replace("CODECS=", "").Trim('"') },
                    { "audio_codec", line2[2].Trim('"') },
                    { "url", streamUrl }
                };
                streamIndex++;
            }

            return streams;
        }



        // PARSE URL TO ID
        private static string GetIDFromUrl(string url)
        {
            List<string> trimFromUrl = new()
            {
                "/",
                "twitch",
                ".",
                "tv",
                "www",
                "https",
                "http",
                ":",
                "videos",
            };
            string id = url;
            foreach (string s in trimFromUrl)
            {
                id = id.Replace(s, "");
            }
            return id;
        }
    }
}