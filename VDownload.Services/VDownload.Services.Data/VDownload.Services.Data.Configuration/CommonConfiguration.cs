using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;
using VDownload.Services.Data.Configuration.Models;

namespace VDownload.Services.Data.Configuration
{
    public class CommonConfiguration
    {
        [ConfigurationKeyName("path")]
        public Models.Path Path { get; set; }

        [ConfigurationKeyName("processing")]
        public Processing Processing { get; set; }

        [ConfigurationKeyName("string_resources_assembly")]
        public string StringResourcesAssembly { get; set; }
    }
}
