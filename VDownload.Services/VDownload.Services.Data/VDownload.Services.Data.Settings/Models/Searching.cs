using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Settings.Models
{
    public class Searching
    {
        [JsonProperty("max_number_of_videos_to_get_from_playlist")]
        public int MaxNumberOfVideosToGetFromPlaylist { get; set; } = 0;
    }
}
