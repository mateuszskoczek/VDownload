using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Request
{
    public class GetClipTokenRequest
    {
        [JsonProperty("operationName")]
        public string OperationName { get; set; }

        [JsonProperty("variables")]
        public GetClipTokenVariables Variables { get; set; }

        [JsonProperty("extensions")]
        public GetClipTokenExtensions Extensions { get; set; }
    }
}
