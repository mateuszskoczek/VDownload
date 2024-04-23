using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Processing
    {
        [ConfigurationKeyName("muxers")]
        public List<Muxer> Muxers { get; } = new List<Muxer>();

        [ConfigurationKeyName("processed_filename")]
        public string ProcessedFilename { get; set; }
    }
}
