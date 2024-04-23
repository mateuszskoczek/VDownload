using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Path
    {
        [ConfigurationKeyName("appdata")]
        public Appdata Appdata { get; set; }

        [ConfigurationKeyName("temp")]
        public Temp Temp { get; set; }
    }
}
