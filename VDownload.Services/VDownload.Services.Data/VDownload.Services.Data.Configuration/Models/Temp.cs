using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Temp
    {
        [ConfigurationKeyName("tasks_directory")]
        public string TasksDirectory { get; set; }
    }
}
