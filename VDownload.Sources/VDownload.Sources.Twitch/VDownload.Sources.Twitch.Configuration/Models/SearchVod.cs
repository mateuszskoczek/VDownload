using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class SearchVod
    {
        [ConfigurationKeyName("regexes")]
        public List<string> Regexes { get; } = new List<string>();

        [ConfigurationKeyName("thumbnail")]
        public Thumbnail Thumbnail { get; set; }

        [ConfigurationKeyName("live_thumbnail_url_regex")]
        public string LiveThumbnailUrlRegex { get; set; }

        [ConfigurationKeyName("stream_playlist_regex")]
        public string StreamPlaylistRegex { get; set; }
    }

}