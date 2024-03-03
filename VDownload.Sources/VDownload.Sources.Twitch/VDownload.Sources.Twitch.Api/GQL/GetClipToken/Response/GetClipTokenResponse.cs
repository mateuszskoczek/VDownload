using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Api.GQL.GetClipToken.Request;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response
{
    public class GetClipTokenResponse
    {
        [JsonProperty("data")]
        public GetClipTokenData Data { get; set; }

        [JsonProperty("extensions")]
        public Response.GetClipTokenExtensions Extensions { get; set; }
    }
}
