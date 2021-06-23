using System;
using System.IO;
using System.Reflection;

namespace VDownload.Global
{
    class Paths
    {
        public static string MAIN = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(@"file:\", "");
        public static string APPDATA = String.Format(@"{0}\VDownload\", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        public static string TEMP = String.Format(@"{0}\VDownload\", Path.GetTempPath());
        public static string OUTPUT = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static string FFMPEG = String.Format(@"{0}\ffmpeg\", MAIN);
    }
}
