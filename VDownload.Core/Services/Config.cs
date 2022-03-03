using System.Collections.Generic;
using VDownload.Core.Enums;
using Windows.Media.Editing;
using Windows.Storage;

namespace VDownload.Core.Services
{
    public class Config
    {
        #region CONSTANTS

        // SETTINGS CONTAINER
        private static readonly ApplicationDataContainer SettingsContainer = ApplicationData.Current.LocalSettings;

        // DEFAULT SETTINGS
        private static readonly Dictionary<string, object> DefaultSettings = new Dictionary<string, object>()
        {
            { "delete_temp_on_start", true },
            { "twitch_vod_passive_trim", true },
            { "twitch_vod_downloading_chunk_retry_after_error", false },
            { "twitch_vod_downloading_chunk_max_retries", 10 },
            { "twitch_vod_downloading_chunk_retries_delay", 5000 },
            { "media_transcoding_use_hardware_acceleration", true },
            { "media_transcoding_use_mrfcrf444_algorithm", true },
            { "media_editing_algorithm", (int)MediaTrimmingPreference.Fast },
            { "default_max_playlist_videos", 0 },
            { "default_media_type", (int)MediaType.AudioVideo },
            { "default_filename", "[<date_pub:yyyy.MM.dd>] <title>" },
            { "default_video_extension", (int)VideoFileExtension.MP4 },
            { "default_audio_extension", (int)AudioFileExtension.MP3 },
            { "custom_media_location", false },
            { "custom_temp_location", false }, 
            { "max_active_video_task", 5 },
            { "replace_output_file_if_exists", false },
            { "remove_task_when_successfully_ended", false },
            { "delete_task_temp_when_ended_with_error", false },
            { "show_notification_when_task_ended_successfully", false },
            { "show_notification_when_task_ended_unsuccessfully", false },
            { "show_warning_when_task_starts_on_metered_network", true },
            { "delay_task_when_queued_task_starts_on_metered_network", true }
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
