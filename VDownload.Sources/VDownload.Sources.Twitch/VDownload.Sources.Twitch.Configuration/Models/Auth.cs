using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Auth
    {
        [ConfigurationKeyName("token_schema")]
        public string TokenSchema { get; set; }

        [ConfigurationKeyName("client_id")]
        public string ClientId { get; set; }

        [ConfigurationKeyName("endpoints")]
        public EndpointsAuth Endpoints { get; set; }
    }

}