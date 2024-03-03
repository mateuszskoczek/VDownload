using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Request
{
    public class GetClipTokenPersistedQuery
    {
        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("sha256Hash")]
        public string Sha256Hash { get; set; }
    }
}
