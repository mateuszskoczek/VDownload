using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiHelixConfiguration
    {
        #region PROPERTIES

        public string TokenSchema { get; protected set; }
        public string ClientId { get; protected set; }
        public TwitchApiHelixEndpointsConfiguration Endpoints { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiHelixConfiguration(IConfigurationSection configuration)
        {
            TokenSchema = configuration["token_schema"];
            ClientId = configuration["client_id"];
            Endpoints = new TwitchApiHelixEndpointsConfiguration(configuration.GetSection("endpoints"));
        }

        #endregion
    }
}
