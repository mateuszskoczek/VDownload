using System.IO;
using System.Collections.Generic;

namespace VDownload.Services
{
    class TempFiles
    {
        // Temporary files list
        public static List<string> List = new();

        // Add to temporary files list
        public static void Add(string file)
        {
            List.Add(file);
        }

        // Delete temporary files from list
        public static void DeleteAll()
        {
            foreach (string f in List)
            {
                File.Delete(f);
            }
            List.Clear();
        }
    }
}
