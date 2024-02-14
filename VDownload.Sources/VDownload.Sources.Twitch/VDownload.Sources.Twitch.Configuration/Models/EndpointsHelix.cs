using Microsoft.Extensions.Configuration;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class EndpointsHelix
    {
        [ConfigurationKeyName("get_videos")]
        public string GetVideos { get; set; }
    }
}