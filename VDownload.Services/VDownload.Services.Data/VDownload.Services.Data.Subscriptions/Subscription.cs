using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Services.Data.Subscriptions
{
    public class Subscription
    {
        #region PROPERTIES

        [JsonProperty("guid")]
        public Guid Guid { get; private set; } = Guid.NewGuid();

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("video_ids")]
        public ICollection<string> VideoIds { get; private set; } = new List<string>();

        #endregion
    }
}
