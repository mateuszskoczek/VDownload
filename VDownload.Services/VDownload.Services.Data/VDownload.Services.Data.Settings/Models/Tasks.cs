using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Services.Data.Settings.Models
{
    public class Tasks
    {
        [JsonProperty("default_media_type")]
        public MediaType DefaultMediaType { get; set; } = MediaType.Original;

        [JsonProperty("default_video_extension")]
        public VideoExtension DefaultVideoExtension { get; set; } = VideoExtension.MP4;

        [JsonProperty("default_audio_extension")]
        public AudioExtension DefaultAudioExtension { get; set; } = AudioExtension.MP3;

        [JsonProperty("filename_template")]
        public string FilenameTemplate { get; set; } = "{title}";

        [JsonProperty("default_output_directory")]
        public string DefaultOutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        [JsonProperty("save_last_output_directory")]
        public bool SaveLastOutputDirectory { get; set; } = false;

        [JsonProperty("max_number_of_running_tasks")]
        public int MaxNumberOfRunningTasks { get; set; } = 5;
    }
}
