using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Settings.Models
{
    public class ChunkDownloadingError
    {
        [JsonProperty("error_retry")]
        public bool Retry { get; set; } = true;

        [JsonProperty("retries_count")]
        public int RetriesCount { get; set; } = 10;

        [JsonProperty("retry_delay")]
        public int RetryDelay { get; set; } = 5000;
    }
}
