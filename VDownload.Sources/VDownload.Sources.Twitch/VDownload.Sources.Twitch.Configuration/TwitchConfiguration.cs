using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Configuration.Models;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchConfiguration
    {
        [ConfigurationKeyName("api")]
        public Api Api { get; set; }

        [ConfigurationKeyName("search")]
        public Search Search { get; set; }

        [ConfigurationKeyName("download")]
        public Download Download { get; set; }

        [ConfigurationKeyName("authentication")]
        public Authentication Authentication { get; set; }
    }
}
