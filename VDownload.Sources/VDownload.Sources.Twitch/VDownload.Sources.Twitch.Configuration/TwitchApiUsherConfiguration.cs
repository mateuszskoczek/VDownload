using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiUsherConfiguration
    {
        #region PROPERTIES

        public TwitchApiUsherEndpointsConfiguration Endpoints { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiUsherConfiguration(IConfigurationSection configuration)
        {
            Endpoints = new TwitchApiUsherEndpointsConfiguration(configuration.GetSection("endpoints"));
        }

        #endregion
    }
}
