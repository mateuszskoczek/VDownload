using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiConfiguration
    {
        #region PROPERTIES

        public TwitchApiAuthConfiguration Auth { get; protected set; }
        public TwitchApiHelixConfiguration Helix { get; protected set; }
        public TwitchApiGQLConfiguration GQL { get; protected set; }
        public TwitchApiUsherConfiguration Usher { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiConfiguration(IConfigurationSection configuration)
        {
            Auth = new TwitchApiAuthConfiguration(configuration.GetSection("auth"));
            Helix = new TwitchApiHelixConfiguration(configuration.GetSection("helix"));
            GQL = new TwitchApiGQLConfiguration(configuration.GetSection("gql"));
            Usher = new TwitchApiUsherConfiguration(configuration.GetSection("usher"));
        }

        #endregion
    }
}
