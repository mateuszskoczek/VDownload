using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiAuthEndpointsConfiguration
    {
        #region PROPERTIES

        public string Validate { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiAuthEndpointsConfiguration(IConfigurationSection configuration)
        {
            Validate = configuration["validate"];
        }

        #endregion
    }
}
