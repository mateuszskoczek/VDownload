using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Authentication.Models
{
    internal class ValidateResponseSuccess
    {
        [JsonProperty("client_id", Required = Required.Always)]
        public string ClientId { get; set; }

        [JsonProperty("login", Required = Required.Always)]
        public string Login { get; set; }

        [JsonProperty("scopes", Required = Required.Always)]
        public List<string> Scopes { get; set; }

        [JsonProperty("user_id", Required = Required.Always)]
        public string UserId { get; set; }

        [JsonProperty("expires_in", Required = Required.Always)]
        public int ExpiresIn { get; set; }
    }
}
