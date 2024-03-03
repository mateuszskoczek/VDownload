using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Settings.Models
{
    public class Notifications
    {
        [JsonProperty("on_successful")]
        public bool OnSuccessful { get; set; } = false;

        [JsonProperty("on_unsuccessful")]
        public bool OnUnsuccessful { get; set; } = false;
    }
}
