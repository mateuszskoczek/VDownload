using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Settings.Models;

namespace VDownload.Sources.Twitch.Settings
{
    public class TwitchSettings
    {
        [JsonProperty("vod")]
        public Vod Vod { get; set; } = new Vod();
    }
}
