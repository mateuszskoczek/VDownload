using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiGQLQueriesConfiguration
    {
        #region PROPERTIES

        public TwitchApiGQLQueriesQueryExtendedConfiguration GetVideoToken { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiGQLQueriesConfiguration(IConfigurationSection configuration)
        {
            GetVideoToken = new TwitchApiGQLQueriesQueryExtendedConfiguration(configuration.GetSection("get_video_token"));
        }

        #endregion
    }
}
