using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Settings.Models;

namespace VDownload.Services.Data.Settings
{
    public class CommonSettings
    {
        [JsonProperty("max_number_of_videos_to_get_from_playlist")]
        public int MaxNumberOfVideosToGetFromPlaylist { get; set; } = 0;

        [JsonProperty("max_number_of_running_tasks")]
        public int MaxNumberOfRunningTasks { get; set; } = 5;

        [JsonProperty("temp_directory")]
        public string TempDirectory { get; set; } = $"{Path.GetTempPath()}\\VDownload";

        [JsonProperty("delete_temp_on_error")]
        public bool DeleteTempOnError { get; set; } = true;

        [JsonProperty("default_task_settings")]
        public DefaultTaskSettings DefaultTaskSettings { get; set; } = new DefaultTaskSettings();

        [JsonProperty("notifications")]
        public Notifications Notifications { get; set; } = new Notifications();

        [JsonProperty("processing")]
        public Processing Processing { get; set; } = new Processing();
    }
}
