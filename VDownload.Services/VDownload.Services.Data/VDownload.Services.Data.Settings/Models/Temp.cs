using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Settings.Models
{
    public class Temp
    {
        [JsonProperty("directory")]
        public string Directory { get; set; } = $"{Path.GetTempPath()}VDownload";

        [JsonProperty("delete_on_error")]
        public bool DeleteOnError { get; set; } = true;
    }
}
