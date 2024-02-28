using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Settings.Models;

namespace VDownload.Services.Data.Settings
{
    public class CommonSettings
    {
        [JsonProperty("max_number_of_videos_to_get_from_playlist")]
        public int MaxNumberOfVideosToGetFromPlaylist { get; set; } = 0;

        [JsonProperty("temp")]
        public Temp Temp { get; set; } = new Temp();

        [JsonProperty("tasks")]
        public Tasks Tasks { get; set; } = new Tasks();

        [JsonProperty("notifications")]
        public Notifications Notifications { get; set; } = new Notifications();

        [JsonProperty("processing")]
        public Processing Processing { get; set; } = new Processing();
    }
}
