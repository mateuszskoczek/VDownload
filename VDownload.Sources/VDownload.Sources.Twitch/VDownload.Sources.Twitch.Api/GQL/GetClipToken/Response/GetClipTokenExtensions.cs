using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response
{
    public class GetClipTokenExtensions
    {
        [JsonProperty("durationMilliseconds")]
        public int DurationMilliseconds { get; set; }

        [JsonProperty("operationName")]
        public string OperationName { get; set; }

        [JsonProperty("requestID")]
        public string RequestID { get; set; }
    }
}
