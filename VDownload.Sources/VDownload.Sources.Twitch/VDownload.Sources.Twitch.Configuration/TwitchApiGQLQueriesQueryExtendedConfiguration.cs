using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiGQLQueriesQueryExtendedConfiguration : TwitchApiGQLQueriesQueryConfiguration
    {
        #region PROPERTIES

        public string OperationName { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiGQLQueriesQueryExtendedConfiguration(IConfigurationSection configuration) : base(configuration)
        {
            OperationName = configuration["operation_name"];
        }

        #endregion
    }
}
