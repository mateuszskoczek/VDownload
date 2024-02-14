using Microsoft.Extensions.Configuration;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class EndpointsUsher
    {
        [ConfigurationKeyName("get_video_playlist")]
        public string GetVideoPlaylist { get; set; }
    }
}