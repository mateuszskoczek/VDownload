using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiHelixEndpointsConfiguration
    {
        #region PROPERTIES

        public string GetVideos { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiHelixEndpointsConfiguration(IConfigurationSection configuration)
        {
            GetVideos = configuration["get_videos"];
        }

        #endregion
    }
}
