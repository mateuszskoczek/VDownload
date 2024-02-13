using Newtonsoft.Json;
namespace VDownload.Sources.Twitch.Api.Helix.GetVideos.Response
{

    public class MutedSegment
    {
        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }

}