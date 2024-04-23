using Microsoft.Extensions.Configuration;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class EndpointsHelix
    {
        [ConfigurationKeyName("get_videos")]
        public string GetVideos { get; set; }

        [ConfigurationKeyName("get_clips")]
        public string GetClips { get; set; }

        [ConfigurationKeyName("get_users")]
        public string GetUsers { get; set; }
    }
}