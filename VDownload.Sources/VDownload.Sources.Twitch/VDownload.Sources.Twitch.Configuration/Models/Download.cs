using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Download
    {
        [ConfigurationKeyName("vod")]
        public DownloadVod Vod { get; set; }
    }

}