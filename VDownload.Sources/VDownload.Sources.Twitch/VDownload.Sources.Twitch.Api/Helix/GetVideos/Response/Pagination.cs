using Newtonsoft.Json;
namespace VDownload.Sources.Twitch.Api.Helix.GetVideos.Response
{

    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }

}