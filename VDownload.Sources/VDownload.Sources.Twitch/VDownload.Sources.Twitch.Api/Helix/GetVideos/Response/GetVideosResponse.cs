using Newtonsoft.Json;
using System.Collections.Generic;

namespace VDownload.Sources.Twitch.Api.Helix.GetVideos.Response
{

    public class GetVideosResponse
    {
        [JsonProperty("data")]
        public List<Data> Data { get; set; }

        [JsonProperty("pagination")]
        public Pagination Pagination { get; set; }
    }

}