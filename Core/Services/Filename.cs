// System
using System.IO;
using System.Collections.Generic;



namespace VDownload.Core.Services
{
    class Filename
    {
        // PROCESS FILENAME (REPLACE WILDCARDS AND FORBIDDEN CHARACTERS)
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
