using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Person
    {
        [ConfigurationKeyName("name")]
        public string Name { get; set; }

        [ConfigurationKeyName("url")]
        public string Url { get; set; }
    }
}
