using System.Collections.Generic;

namespace VDownload.Global
{
    class ProgramInfo
    {
        public static readonly string VERSION = "0.3-dev2";
        public static readonly string BUILD_ID = "21178";
        public static readonly string REPOSITORY = "https://github.com/mateuszskoczek/VDownload";
        public static readonly string AUTHOR_NAME = "Mateusz Skoczek";
        public static readonly string AUTHOR_GITHUB = "https://github.com/mateuszskoczek";
        public static readonly string DONATION_LINK = "https://paypal.me/mateuszskoczek";
        public static readonly string PROJECT_START = "Feb 2021";
        public static readonly string PROJECT_END = "Jul 2021";
        public static readonly string RELEASES = "https://github.com/mateuszskoczek/VDownload/releases";
        public static readonly Dictionary<string, string> LIBRARIES = new()
        {
            { "ConsoleTableExt 3.1.7", "https://github.com/minhhungit/ConsoleTableExt" },
            { "FFmpegCore 4.3.0", "https://github.com/rosenbjerg/FFMpegCore" },
            { "LightConfig 0.3.0", "https://github.com/mateuszskoczek/LightConfig" },
            { "YoutubeExplode 6.0.3", "https://github.com/Tyrrrz/YoutubeExplode" },
            { "Octokit 0.50.0", "https://github.com/octokit/octokit.net" },
            { "Newtonsoft.Json 13.0.1", "https://www.newtonsoft.com/json" },
        };
    }
}