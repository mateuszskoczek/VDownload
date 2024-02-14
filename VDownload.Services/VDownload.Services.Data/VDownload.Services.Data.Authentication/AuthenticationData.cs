using Newtonsoft.Json;
using VDownload.Services.Data.Authentication.Models;

namespace VDownload.Services.Data.Authentication
{
    public class AuthenticationData
    {
        #region PROPERTIES

        [JsonProperty("twitch")]
        public TokenAuthenticationData Twitch { get; set; }

        #endregion



        #region CONSTRUCTORS

        public AuthenticationData()
        {
            Twitch = new TokenAuthenticationData();
        }

        #endregion
    }
}