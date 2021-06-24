using System;
using System.IO;
using System.Reflection;

namespace VDownload.Global
{
    class Paths
    {
        public static readonly string MAIN = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(@"file:\", "");
        public static readonly string APPDATA = String.Format(@"{0}\VDownload\", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
        public static readonly string TEMP = String.Format(@"{0}\VDownload\", Path.GetTempPath());
        public static readonly string OUTPUT = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string FFMPEG = String.Format(@"{0}\ffmpeg\", MAIN);
    }
}
