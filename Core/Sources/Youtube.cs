// System
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

// External
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

// Internal
using VDownload.Core.Services;



namespace VDownload.Core.Sources
{
    class Youtube
    {
        #region CONSTANTS

        private static readonly YoutubeClient Client = new();

        #endregion



        #region MAIN

        // GET METADATA
        public static async Task<Dictionary<string, string>> GetMetadata(string url)
        {
            // Request
            var response = await Client.Videos.GetAsync(url);

            // Pack into dictionary
            Dictionary<string, string> metadata = new();
            metadata["title"] = response.Title;
            metadata["author_name"] = response.Author.Title;
            metadata["author_id"] = response.Author.ChannelId;
            metadata["date"] = response.UploadDate.Date.ToString(Config.Read("date_format"));
            metadata["duration"] = response.Duration.ToString();
            metadata["views"] = response.Engagement.ViewCount.ToString();
            metadata["likes"] = response.Engagement.LikeCount.ToString();
            metadata["dislikes"] = response.Engagement.DislikeCount.ToString();
            metadata["likes_procent"] = ((response.Engagement.LikeCount * 100) / (response.Engagement.LikeCount + response.Engagement.DislikeCount)).ToString();
            metadata["thumbnail"] = String.Format(@"https://img.youtube.com/vi/{0}/maxresdefault.jpg", response.Id);
            metadata["description"] = response.Description;
            metadata["url"] = response.Url;
            metadata["id"] = response.Id;

            // Return metadata
            return metadata;
        }


        // GET VIDEO STREAMS
        public static async Task<Dictionary<int, Tuple<Dictionary<string, string>, VideoOnlyStreamInfo>>> GetVideoStreams(string url)
        {
            // Request
            var response = await Client.Videos.Streams.GetManifestAsync(url);
            var streamObjects = response.GetVideoOnlyStreams();

            // Pack into dictionary
            Dictionary<int, Tuple<Dictionary<string, string>, VideoOnlyStreamInfo>> streams = new();
            int i = 0;
            foreach (var s in streamObjects)
            {
                Dictionary<string, string> metadata = new();
                metadata["bitrate"] = s.Bitrate.ToString();
                metadata["bitrate_base"] = s.Bitrate.BitsPerSecond.ToString();
                metadata["container"] = s.Container.Name;
                metadata["quality"] = s.VideoQuality.Label;
                metadata["codec"] = s.VideoCodec;
                metadata["size"] = s.Size.ToString();
                streams[i] = Tuple.Create(metadata, s);
                i++;
            }

            // Return streams
            return streams;
        }


        // GET AUDIO STREAMS
        public static async Task<Dictionary<int, Tuple<Dictionary<string, string>, AudioOnlyStreamInfo>>> GetAudioStreams(string url)
        {
            // Request
            var response = await Client.Videos.Streams.GetManifestAsync(url);
            var streamObjects = response.GetAudioOnlyStreams();

            // Pack into dictionary
            Dictionary<int, Tuple<Dictionary<string, string>, AudioOnlyStreamInfo>> streams = new();
            int i = 0;
            foreach (var s in streamObjects)
            {
                Dictionary<string, string> metadata = new();
                metadata["bitrate"] = s.Bitrate.ToString();
                metadata["bitrate_base"] = s.Bitrate.BitsPerSecond.ToString();
                metadata["container"] = s.Container.Name;
                metadata["codec"] = s.AudioCodec;
                metadata["size"] = s.Size.ToString();
                streams[i] = Tuple.Create(metadata, s);
                i++;
            }

            // Return streams
            return streams;
        }


        // DOWNLOAD VIDEO STREAM
        public static async Task DownloadVideoStream(VideoOnlyStreamInfo stream, string path)
        {
            // Create temp directory if not exist
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Download
            await Client.Videos.Streams.DownloadAsync(stream, path);
        }


        // DOWNLOAD AUDIO STREAM
        public static async Task DownloadAudioStream(AudioOnlyStreamInfo stream, string path)
        {
            // Create temp directory if not exist
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            // Download
            await Client.Videos.Streams.DownloadAsync(stream, path);
        }


        // SELECT BEST STREAM
        public static int SelectBestStream(Dictionary<int, Dictionary<string, string>> streams)
        {
            // Max variables
            double max_bitrate_val = 0;
            int max_bitrate_id = 0;

            // Check each stream
            foreach (var s in streams)
            {
                int id = s.Key;
                double bitrate = int.Parse(s.Value["bitrate_base"]);

                // Check if max
                if (bitrate > max_bitrate_val)
                {
                    max_bitrate_val = bitrate;
                    max_bitrate_id = id;
                }
            }

            // Return id
            return max_bitrate_id;
        }

        #endregion
    }
}
