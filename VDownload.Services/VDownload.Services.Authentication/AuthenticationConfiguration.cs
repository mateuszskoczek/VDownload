using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Authentication
{
    public class AuthenticationConfiguration
    {
        #region PROPERTIES

        public string FilePath { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public AuthenticationConfiguration(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection("authentication");
            FilePath = section["file_path"];
        }

        #endregion
    }
}
