using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Configuration
{
    public class TwitchAuthenticationConfiguration
    {
        #region PROPERTIES

        public string Url { get; protected set; }
        public string RedirectUrl { get; protected set; }
        public Regex RedirectUrlRegex { get; protected set; }
        public string ClientId { get; protected set; }
        public string ResponseType { get; protected set; }
        public IEnumerable<string> Scopes { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchAuthenticationConfiguration(IConfigurationSection configuration)
        {
            Url = configuration["url"];
            RedirectUrl = configuration["redirect_url"];
            RedirectUrlRegex = new Regex(configuration["redirect_url_regex"]);
            ClientId = configuration["client_id"];
            ResponseType = configuration["response_type"];
            Scopes = configuration.GetSection("scopes").Get<IEnumerable<string>>();
        }

        #endregion
    }
}
