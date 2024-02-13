using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Authentication.Models
{
    internal class ValidateResponseFail
    {
        [JsonProperty("status", Required = Required.Always)]
        public int Status { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; set; }
    }
}
