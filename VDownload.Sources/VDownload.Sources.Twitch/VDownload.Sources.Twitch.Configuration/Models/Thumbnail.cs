using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Thumbnail
    {
        [ConfigurationKeyName("width")]
        public int Width { get; set; }

        [ConfigurationKeyName("height")]
        public int Height { get; set; }
    }

}