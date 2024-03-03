using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response
{
    public class GetClipTokenVideoQuality
    {
        [JsonProperty("frameRate")]
        public double FrameRate { get; set; }

        [JsonProperty("quality")]
        public string Quality { get; set; }

        [JsonProperty("sourceURL")]
        public string SourceURL { get; set; }

        [JsonProperty("__typename")]
        public string Typename { get; set; }
    }
}
