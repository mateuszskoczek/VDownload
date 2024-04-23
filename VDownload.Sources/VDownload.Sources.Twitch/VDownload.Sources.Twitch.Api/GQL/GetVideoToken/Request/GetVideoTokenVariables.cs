using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Search.Models.GetVideoToken.Request
{
    public class GetVideoTokenVariables
    {
        [JsonProperty("isLive")]
        public bool IsLive { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [JsonProperty("isVod")]
        public bool IsVod { get; set; }

        [JsonProperty("vodID")]
        public string VodID { get; set; }

        [JsonProperty("playerType")]
        public string PlayerType { get; set; }
    }
}
