using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiAuthConfiguration
    {
        #region PROPERTIES

        public string TokenSchema { get; protected set; }
        public string ClientId { get; protected set; }
        public TwitchApiAuthEndpointsConfiguration Endpoints { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiAuthConfiguration(IConfigurationSection configuration)
        {
            TokenSchema = configuration["token_schema"];
            ClientId = configuration["client_id"];
            Endpoints = new TwitchApiAuthEndpointsConfiguration(configuration.GetSection("endpoints"));
        }

        #endregion
    }
}
