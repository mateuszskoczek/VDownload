using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using VDownload.Parsers;

namespace VDownload
{
    class Twitch
    {
        private static readonly WebClient Client = new();

        public static void VodInfo(string url)
        {
            string output;
            // Get metadata
            var metadata = GetVodMetadata(url);

            // Get streams
            string streams = "test";

            // Output
            output = TerminalOutput.Get(@"output\twitch\info\vod\info.out",
                args: new()
                {
                    metadata["title"],
                    metadata["url"],
                    metadata["author"],
                    metadata["views"],
                    metadata["date"],
                    metadata["duration"],
                    metadata["thumbnail"],
                    streams,
                }
            );
            Console.WriteLine(output);
        }

        private static Dictionary<string, string> GetVodMetadata(string url)
        {
            // Client settings
            Client.Headers.Add("Accept", "application/vnd.twitchtv.v5+json");
            Client.Headers.Add("Client-ID", "kimne78kx3ncx6brgo4mv6wki5h1ko");

            // Get metadata
            var vod = JObject.Parse(Client.DownloadString(String.Format("https://api.twitch.tv/kraken/videos/{0}", GetIDFromUrl(url))));

            Dictionary<string, string> metadata = new();
            metadata["title"] = vod["title"].ToString().Replace("\n", "");
            metadata["author"] = vod["channel"]["display_name"].ToString();
            metadata["date"] = Convert.ToDateTime(vod["created_at"].ToString()).ToString(Config.Main.ReadKey("date_format"));
            metadata["duration"] = Duration.ParseSeconds(int.Parse(vod["length"].ToString()));
            metadata["views"] = vod["views"].ToString();
            metadata["thumbnail"] = vod["thumbnails"]["large"][0]["url"].ToString();
            metadata["url"] = vod["url"].ToString();
            metadata["id"] = vod["_id"].ToString().Trim('v');

            return metadata;
        }

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