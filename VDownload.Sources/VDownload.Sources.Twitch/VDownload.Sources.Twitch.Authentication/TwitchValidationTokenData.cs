using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Authentication.Models;

namespace VDownload.Sources.Twitch.Authentication
{
    public class TwitchValidationTokenData
    {
        #region PROPERTIES

        public string ClientId { get; private set; }
        public string Login { get; private set; }
        public IEnumerable<string> Scopes { get; private set; }
        public string UserId { get; private set; }
        public DateTime ExpirationDate { get; private set; }

        #endregion



        #region CONSTRUCTORS

        internal TwitchValidationTokenData(ValidateResponseSuccess response) 
        {
            ClientId = response.ClientId;
            Login = response.Login;
            Scopes = response.Scopes;
            UserId = response.UserId;
            ExpirationDate = DateTime.Now.AddSeconds(response.ExpiresIn);
        }

        #endregion
    }
}
