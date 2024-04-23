using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Services.Data.Settings.Models
{
    public class Processing
    {
        [JsonProperty("ffmpeg_location")]
        public string FFmpegLocation { get; set; } = $"{AppDomain.CurrentDomain.BaseDirectory}\\FFmpeg";

        [JsonProperty("use_multithreading")]
        public bool UseMultithreading { get; set; } = true;

        [JsonProperty("use_hardware_acceleration")]
        public bool UseHardwareAcceleration { get; set; } = true;

        [JsonProperty("speed")]
        public ProcessingSpeed Speed { get; set; } = ProcessingSpeed.UltraFast;
    }
}
