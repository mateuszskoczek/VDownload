using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Editing;
using Windows.Storage;

namespace VDownload.Core.Services
{
    internal class Config
    {
        #region CONSTANTS

        // SETTINGS CONTAINER
        private static readonly ApplicationDataContainer SettingsContainer = ApplicationData.Current.LocalSettings;

        // DEFAULT SETTINGS
        private static readonly Dictionary<string, object> DefaultSettings = new Dictionary<string, object>()
        {
            { "delete_temp_on_start", true },
            { "twitch_vod_passive_trim", true },
            { "twitch_vod_downloading_chunk_retry_after_error", true },
            { "twitch_vod_downloading_chunk_max_retries", 10 },
            { "twitch_vod_downloading_chunk_retries_delay", 5000 },
            { "media_transcoding_use_hardware_acceleration", true },
            { "media_transcoding_use_mrfcrf444_algorithm", true },
            { "media_editing_algorithm", MediaTrimmingPreference.Fast }
        };

        #endregion



        #region METHODS

        // GET VALUE
        public static object GetValue(string key)
        {
            return SettingsContainer.Values[key];
        }

        // SET VALUE
        public static void SetValue(string key, object value)
        {
            SettingsContainer.Values[key] = value;
        }

        // SET DEFAULT
        public static void SetDefault()
        {
            foreach (KeyValuePair<string, object> s in DefaultSettings)
            {
                SettingsContainer.Values[s.Key] = s.Value;
            }
        }

        // REBUILD
        public static void Rebuild()
        {
            foreach (KeyValuePair<string, object> s in DefaultSettings)
            {
                if (!SettingsContainer.Values.ContainsKey(s.Key))
                {
                    SettingsContainer.Values[s.Key] = s.Value;
                }
            }
        }

        #endregion
    }
}
