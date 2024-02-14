using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Usher
    {
        [ConfigurationKeyName("endpoints")]
        public EndpointsUsher Endpoints { get; set; }
    }

}