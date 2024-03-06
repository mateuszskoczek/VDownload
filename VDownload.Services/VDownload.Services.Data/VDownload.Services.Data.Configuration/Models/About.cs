using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Configuration.Models
{
    public class About
    {
        [ConfigurationKeyName("repository_url")]
        public string RepositoryUrl { get; set; }

        [ConfigurationKeyName("donation_url")]
        public string DonationUrl { get; set; }

        [ConfigurationKeyName("developers")]
        public IEnumerable<Person> Developers { get; set; }

        [ConfigurationKeyName("translation")]
        public IEnumerable<Language> Translation { get; set; }
    }
}
