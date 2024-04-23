using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Configuration;

namespace VDownload.Services.Data.Configuration
{
    public interface IConfigurationService
    {
        CommonConfiguration Common { get; }
        TwitchConfiguration Twitch { get; }
    }



    public class ConfigurationService : IConfigurationService
    {
        #region SERVICES

        protected readonly IConfiguration _configuration;

        #endregion



        #region PROPERTIES

        public CommonConfiguration Common { get; protected set; }
        public TwitchConfiguration Twitch { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public ConfigurationService(IConfiguration configuration)
        {
            Common = configuration.GetSection("common").Get<CommonConfiguration>()!;
            Twitch = configuration.GetSection("twitch").Get<TwitchConfiguration>()!;
        }

        #endregion
    }
}
