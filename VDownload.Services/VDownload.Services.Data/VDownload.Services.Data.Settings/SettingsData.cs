using Newtonsoft.Json;
using VDownload.Sources.Twitch.Settings;

namespace VDownload.Services.Data.Settings
{
    public class SettingsData
    {
        #region PROPERTIES

        [JsonProperty("common")]
        public CommonSettings Common { get; set; }

        [JsonProperty("twitch")]
        public TwitchSettings Twitch { get; set; }

        #endregion



        #region CONSTRUCTORS

        internal SettingsData() 
        {
            Common = new CommonSettings();
            Twitch = new TwitchSettings();
        }

        #endregion
    }
}
