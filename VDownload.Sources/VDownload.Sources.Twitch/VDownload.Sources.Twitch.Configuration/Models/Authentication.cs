using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class Authentication
    {
        [ConfigurationKeyName("url")]
        public string Url { get; set; }

        [ConfigurationKeyName("redirect_url")]
        public string RedirectUrl { get; set; }

        [ConfigurationKeyName("redirect_url_regex")]
        public string RedirectUrlRegex { get; set; }

        [ConfigurationKeyName("client_id")]
        public string ClientId { get; set; }

        [ConfigurationKeyName("response_type")]
        public string ResponseType { get; set; }

        [ConfigurationKeyName("scopes")]
        public List<string> Scopes { get; } = new List<string>();
    }
}
