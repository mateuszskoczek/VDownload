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
        [JsonProperty("searching")]
        public Searching Searching { get; set; } = new Searching();

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
