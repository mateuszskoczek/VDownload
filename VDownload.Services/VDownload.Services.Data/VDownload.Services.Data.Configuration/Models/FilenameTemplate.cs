using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class FilenameTemplate
    {
        [ConfigurationKeyName("name")]
        public string Name { get; set; }

        [ConfigurationKeyName("wildcard")]
        public string Wildcard { get; set; }
    }
}
