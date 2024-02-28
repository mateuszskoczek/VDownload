using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class Channel
    {
        [ConfigurationKeyName("regexes")]
        public List<string> Regexes { get; } = new List<string>();

        [ConfigurationKeyName("url")]
        public string Url { get; set; }
    }
}
