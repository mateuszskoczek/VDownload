using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class EndpointsAuth
    {
        [ConfigurationKeyName("validate")]
        public string Validate { get; set; }
    }

}