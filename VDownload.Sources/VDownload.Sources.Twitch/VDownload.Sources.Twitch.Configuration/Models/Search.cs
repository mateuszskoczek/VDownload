using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Search
    {
        [ConfigurationKeyName("general_regexes")]
        public List<string> GeneralRegexes { get; } = new List<string>();

        [ConfigurationKeyName("vod")]
        public Vod Vod { get; set; }

        [ConfigurationKeyName("clip")]
        public Clip Clip { get; set; }

        [ConfigurationKeyName("channel")]
        public Channel Channel { get; set; }
    }

}