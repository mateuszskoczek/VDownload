using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Application
{
    public class ApplicationData
    {
        [JsonProperty("common")]
        public CommonApplicationData Common { get; set; }
    }
}
