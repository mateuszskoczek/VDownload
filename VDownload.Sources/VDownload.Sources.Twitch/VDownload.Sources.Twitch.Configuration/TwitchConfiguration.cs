using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchConfiguration
    {
        #region PROPERTIES

        public TwitchApiConfiguration Api { get; protected set; }
        public TwitchSearchConfiguration Search { get; protected set; }
        public TwitchAuthenticationConfiguration Authentication { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public TwitchConfiguration(IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection("sources").GetSection("twitch");
            Api = new TwitchApiConfiguration(section.GetSection("api"));
            Search = new TwitchSearchConfiguration(section.GetSection("search"));
            Authentication = new TwitchAuthenticationConfiguration(section.GetSection("authentication"));
        }

        #endregion
    }
}
