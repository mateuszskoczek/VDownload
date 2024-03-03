using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Request
{
    public class GetClipTokenExtensions
    {
        [JsonProperty("persistedQuery")]
        public GetClipTokenPersistedQuery PersistedQuery { get; set; }
    }
}
