// System
using System.Collections.Generic;
using Windows.Storage;

namespace VDownload.Core.Services
{
    class Config
    {
        #region CONSTANTS

        public static readonly string[] VideoExtensionList = new string[]
        {
            "MP4",
            "AVI",
            "MOV",
        };
        public static readonly string[] AudioExtensionList = new string[]
        {
            "MP3",
            "FLAC",
        };
        public static readonly string[] DateFormatList = new string[]
        {
            "YYYY.MM.DD",
            "YYYY.DD.MM",
            "DD.MM.YYYY",
            "MM.DD.YYYY",
        };
        private static readonly ApplicationDataContainer SettingsContainer = ApplicationData.Current.LocalSettings;
        private static readonly Dictionary<string, object> DefaultSettings = new Dictionary<string, object>()
        {
            { "ffmpeg_executables_path", "" },
            { "max_downloaded_videos", 5 },
            { "max_downloaded_chunks", 25 },
            { "default_output_filename", "%title%" },
            { "default_output_video_extension", VideoExtensionList[0] },
            { "default_output_audio_extension", AudioExtensionList[0] },
            { "date_format", DateFormatList[0] },
        };

        #endregion



        #region MAIN

        // GET VALUE
        public static object GetValue(string key)
        {
            return SettingsContainer.Values[key].ToString();
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
