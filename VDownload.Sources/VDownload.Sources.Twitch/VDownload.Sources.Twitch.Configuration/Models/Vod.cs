using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Vod
    {
        [ConfigurationKeyName("regexes")]
        public List<string> Regexes { get; } = new List<string>();

        [ConfigurationKeyName("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [ConfigurationKeyName("stream_playlist_regex")]
        public string StreamPlaylistRegex { get; set; }

        [ConfigurationKeyName("chunk_regex")]
        public string ChunkRegex { get; set; }

        [ConfigurationKeyName("file_name")]
        public string FileName { get; set; }
    }

}