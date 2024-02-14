using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization; 
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Api
    {
        [ConfigurationKeyName("auth")]
        public Auth Auth { get; set; }

        [ConfigurationKeyName("helix")]
        public Helix Helix { get; set; }

        [ConfigurationKeyName("gql")]
        public Gql Gql { get; set; }

        [ConfigurationKeyName("usher")]
        public Usher Usher { get; set; }
    }

}