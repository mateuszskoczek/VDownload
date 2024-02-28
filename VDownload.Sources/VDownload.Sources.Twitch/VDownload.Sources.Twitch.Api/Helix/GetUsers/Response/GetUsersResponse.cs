using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Api.Helix.GetUsers.Response
{
    public class GetUsersResponse
    {
        [JsonProperty("data")]
        public List<Data> Data { get; } = new List<Data>();
    }
}
