using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration.Models
{
    public class GetClipToken
    {
        [ConfigurationKeyName("operation_name")]
        public string OperationName { get; set; }

        [ConfigurationKeyName("persisted_query_version")]
        public int PersistedQueryVersion { get; set; }

        [ConfigurationKeyName("persisted_query_hash")]
        public string PersistedQueryHash { get; set; }
    }
}
