using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class Language
    {
        [ConfigurationKeyName("code")]
        public string Code { get; set; }

        [ConfigurationKeyName("translators")]
        public IEnumerable<Person> Translators { get; set; }
    }
}
