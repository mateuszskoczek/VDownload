// System
using System.Collections.Generic;
using Windows.Storage;

namespace VDownload.Services
{
    internal class Config
    {
        #region CONSTANTS

        // DATA VALUES LISTS
        public static readonly string[] DateFormatList = new string[]
        {
            "yyyy.MM.dd",
            "yyyy.dd.MM",
            "dd.MM.yyyy",
            "MM.dd.yyyy",
        };
        public static readonly string[] DefaultMediaTypeList = new string[]
        {
            "AV",
            "A",
            "V"
        };
        public static readonly string[] DefaultVideoExtensionList = new string[]
        {
            "MP4",
            "WMV",
            "HEVC"
        };
        public static readonly string[] DefaultAudioExtensionList = new string[]
        {
            "MP3",
            "FLAC",
            "WAV",
            "M4A",
            "ALAC",
            "WMA"
        };

        // DEFAULT SETTINGS
        private static readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>()
        {
            { "max_video_tasks" , "5" },
            { "default_video_extension", DefaultVideoExtensionList[0] },
            { "default_audio_extension", DefaultAudioExtensionList[0] },
            { "date_format", DateFormatList[0] },
            { "default_media_type", DefaultMediaTypeList[0] },
            { "default_output_filename", "[%date_pub%] %title%" },
            { "use_mrfcrf444", "1" },
            { "use_hardware_acceleration", "1" },
            { "delete_temp_on_start", "1" },
            { "delete_video_temp_after_error", "1" },
            { "max_playlist_videos", "0" },
        };

        // SETTINGS CONTAINER
        private static readonly ApplicationDataContainer SettingsContainer = ApplicationData.Current.LocalSettings;

        #endregion



        #region MAIN

        // GET VALUE
        public static string GetValue(string key)
        {
            return SettingsContainer.Values[key].ToString();
        }

        // SET VALUE
        public static void SetValue(string key, string value)
        {
            SettingsContainer.Values[key] = value;
        }

        // SET DEFAULT
        public static void SetDefault()
        {
            foreach (KeyValuePair<string, string> s in DefaultSettings)
            {
                SettingsContainer.Values[s.Key] = s.Value;
            }
        }


        // REBUILD
        public static void Rebuild()
        {
            foreach (KeyValuePair<string, string> s in DefaultSettings)
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
