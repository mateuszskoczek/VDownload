using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Queries
    {
        [ConfigurationKeyName("get_video_token")]
        public GetVideoToken GetVideoToken { get; set; }

        [ConfigurationKeyName("get_clip_token")]
        public GetClipToken GetClipToken { get; set; }
    }

}