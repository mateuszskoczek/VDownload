using System;
using System.Collections.Generic;
using LightConfig;

namespace VDownload
{
    static class Config
    {
        // MAIN CONFIGURATION FILE (config.cfg)
        private static readonly string MainPath = String.Format(@"{0}\config.cfg", Global.Paths.APPDATA);
        private static readonly Dictionary<string, string> MainContent = new()
        {
            {"filename", "%title%"},
            {"output_path", Global.Paths.OUTPUT},
            {"video_ext", "mp4"},
            {"audio_ext", "mp3"},
            {"date_format", "yyyy.MM.dd"},
            {"ffmpeg_path", Global.Paths.FFMPEG},
            {"check_updates_on_start", "1"},
        };
        public static ConfigObject Main = new(MainPath, MainContent);
    }
}
