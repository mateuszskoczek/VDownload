using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response
{
    public class GetClipTokenPlaybackAccessToken
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("__typename")]
        public string Typename { get; set; }
    }
}
