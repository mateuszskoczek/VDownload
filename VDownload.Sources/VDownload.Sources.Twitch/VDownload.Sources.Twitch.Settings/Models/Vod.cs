using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Settings.Models
{
    public class Vod
    {
        [JsonProperty("passive_trimming")]
        public bool PassiveTrimming { get; set; } = true;

        [JsonProperty("chunk_downloading_error")]
        public ChunkDownloadingError ChunkDownloadingError { get; set; } = new ChunkDownloadingError();

        [JsonProperty("max_number_of_parallel_downloads")]
        public int MaxNumberOfParallelDownloads { get; set; } = 100;
    }
}
