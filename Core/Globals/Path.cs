// System
using System;
using System.Reflection;



namespace VDownload.Core.Globals
{
    class Path
    {
        public static readonly string Main = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace(@"file:\", "");
        public static readonly string AppData = $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\VDownload\";
        public static readonly string Temp = $@"{System.IO.Path.GetTempPath()}\VDownload\";
    }
}
