using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiGQLConfiguration
    {
        #region PROPERTIES

        public string ClientId { get; protected set; }
        public string Endpoint { get; protected set; }
        public TwitchApiGQLQueriesConfiguration Queries { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiGQLConfiguration(IConfigurationSection configuration)
        {
            ClientId = configuration["client_id"];
            Endpoint = configuration["endpoint"];
            Queries = new TwitchApiGQLQueriesConfiguration(configuration.GetSection("queries"));
        }

        #endregion
    }
}
