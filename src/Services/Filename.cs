using System.IO;
using System.Collections.Generic;

namespace VDownload.Services
{
    class Filename
    {
        // Process filename (replace wildcards and forbidden chars)
        public static string Process(string filename, Dictionary<string, string> data)
        {
            // Wildcards replace
            foreach (var e in data)
            {
                filename = filename.Replace(e.Key, e.Value);
            }

            // Forbidden chars replace
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c.ToString(), "");
            }

            // Return filename
            return filename;
        }
    }
}
