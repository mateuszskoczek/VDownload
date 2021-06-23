using System.Collections.Generic;

namespace VDownload.Global
{
    class ProgramInfo
    {
        public static string VERSION = "0.2-dev7";
        public static string BUILD_ID = "21174";
        public static string REPOSITORY = "https://github.com/mateuszskoczek/VDownload";
        public static string AUTHOR_NAME = "Mateusz Skoczek";
        public static string AUTHOR_GITHUB = "https://github.com/mateuszskoczek";
        public static string DONATION_LINK = "https://paypal.me/mateuszskoczek";
        public static string PROJECT_START = "Feb 2021";
        public static string PROJECT_END = "Jul 2021";
        public static Dictionary<string, string> LIBRARIES = new()
        {
            { "ConsoleTableExt 3.1.7", "https://github.com/minhhungit/ConsoleTableExt" },
            { "FFmpegCore 4.3.0", "https://github.com/rosenbjerg/FFMpegCore" },
            { "LightConfig 0.2.0", "https://github.com/mateuszskoczek/LightConfig" },
            { "YoutubeExplode 6.0.3", "https://github.com/Tyrrrz/YoutubeExplode" },
            { "Octokit 0.50.0", "https://github.com/octokit/octokit.net" },
        };
    }
}