using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Appdata
    {
        [ConfigurationKeyName("directory_name")]
        public string DirectoryName { get; set; }

        [ConfigurationKeyName("authentication_file")]
        public string AuthenticationFile { get; set; }

        [ConfigurationKeyName("settings_file")]
        public string SettingsFile { get; set; }
    }
}
