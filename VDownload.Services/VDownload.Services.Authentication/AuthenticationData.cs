using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Authentication
{
    public class AuthenticationData
    {
        #region PROPERTY

        [JsonProperty("twitch")]
        public AuthenticationDataTwitch Twitch { get; internal set; }

        #endregion



        #region CONSTRUCTORS

        public AuthenticationData() 
        {
            Twitch = new AuthenticationDataTwitch();
        }

        #endregion
    }
}
