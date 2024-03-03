using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Api.Helix.GetVideos.Response;

namespace VDownload.Sources.Twitch.Api.Helix.GetClips.Response
{
    public class GetClipsResponse
    {
        [JsonProperty("data")]
        public List<Data> Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }
}
