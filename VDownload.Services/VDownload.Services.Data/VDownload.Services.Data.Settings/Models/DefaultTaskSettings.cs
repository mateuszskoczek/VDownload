using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Services.Data.Settings.Models
{
    public class DefaultTaskSettings
    {
        [JsonProperty("media_type")]
        public MediaType MediaType { get; set; } = MediaType.Original;

        [JsonProperty("video_extension")]
        public VideoExtension VideoExtension { get; set; } = VideoExtension.MP4;

        [JsonProperty("audio_extension")]
        public AudioExtension AudioExtension { get; set; } = AudioExtension.MP3;

        [JsonProperty("output_directory")]
        public string OutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        [JsonProperty("save_last_output_directory")]
        public bool SaveLastOutputDirectory { get; set; } = false;
    }
}
