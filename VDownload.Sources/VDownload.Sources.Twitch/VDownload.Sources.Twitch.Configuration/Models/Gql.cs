using Microsoft.Extensions.Configuration;
namespace VDownload.Sources.Twitch.Configuration.Models{ 

    public class Gql
    {
        [ConfigurationKeyName("client_id")]
        public string ClientId { get; set; }

        [ConfigurationKeyName("endpoint")]
        public string Endpoint { get; set; }

        [ConfigurationKeyName("queries")]
        public Queries Queries { get; set; }
    }

}