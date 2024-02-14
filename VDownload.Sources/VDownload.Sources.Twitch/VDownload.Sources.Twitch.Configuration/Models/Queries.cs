using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Queries
    {
        [ConfigurationKeyName("get_video_token")]
        public GetVideoToken GetVideoToken { get; set; }
    }

}