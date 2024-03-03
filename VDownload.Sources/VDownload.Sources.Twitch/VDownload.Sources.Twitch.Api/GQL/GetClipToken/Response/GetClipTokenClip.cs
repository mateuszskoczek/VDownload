using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response
{
    public class GetClipTokenClip
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("playbackAccessToken")]
        public GetClipTokenPlaybackAccessToken PlaybackAccessToken { get; set; }

        [JsonProperty("videoQualities")]
        public List<GetClipTokenVideoQuality> VideoQualities { get; set; }

        [JsonProperty("__typename")]
        public string Typename { get; set; }
    }
}
