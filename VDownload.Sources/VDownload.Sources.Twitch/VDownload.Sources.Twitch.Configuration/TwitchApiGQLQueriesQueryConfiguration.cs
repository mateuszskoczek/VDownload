using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiGQLQueriesQueryConfiguration
    {
        #region PROPERTIES

        public string Query { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiGQLQueriesQueryConfiguration(IConfigurationSection configuration)
        {
            Query = configuration["query"];
        }

        #endregion
    }
}
