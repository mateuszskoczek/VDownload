using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Muxer
    {
        [ConfigurationKeyName("extension")]
        public string Extension { get; set; }

        [ConfigurationKeyName("video_codecs")]
        public List<string> VideoCodecs { get; } = new List<string>();

        [ConfigurationKeyName("audio_codecs")]
        public List<string> AudioCodecs { get; } = new List<string>();
    }
}
