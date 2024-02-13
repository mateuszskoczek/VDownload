using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchApiUsherEndpointsConfiguration
    {
        #region PROPERTIES

        public string GetVideoPlaylist { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchApiUsherEndpointsConfiguration(IConfigurationSection configuration)
        {
            GetVideoPlaylist = configuration["get_video_playlist"];
        }

        #endregion
    }
}
