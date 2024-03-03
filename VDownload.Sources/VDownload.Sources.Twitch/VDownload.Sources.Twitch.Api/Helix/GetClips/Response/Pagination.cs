using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.Helix.GetClips.Response
{
    public class Pagination
    {
        [JsonProperty("cursor")]
        public string Cursor { get; set; }
    }
}
