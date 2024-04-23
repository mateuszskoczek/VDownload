using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class DownloadVod
    {
        [ConfigurationKeyName("chunk_regex")]
        public string ChunkRegex { get; set; }

        [ConfigurationKeyName("file_name")]
        public string FileName { get; set; }
    }
}
