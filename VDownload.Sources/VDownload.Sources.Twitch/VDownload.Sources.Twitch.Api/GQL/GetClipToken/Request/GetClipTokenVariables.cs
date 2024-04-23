using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Request
{
    public class GetClipTokenVariables
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }
    }
}
