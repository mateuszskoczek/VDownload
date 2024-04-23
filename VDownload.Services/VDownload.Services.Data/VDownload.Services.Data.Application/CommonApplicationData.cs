using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Application
{
    public class CommonApplicationData
    {
        [JsonProperty("last_output_directory")]
        public string? LastOutputDirectory { get; set; } = null;
    }
}
