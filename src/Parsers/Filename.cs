using System.IO;
using System.Collections.Generic;

namespace VDownload.Parsers
{
    class Filename
    {
        public static string Get(string filename, Dictionary<string, string> data)
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

            return filename;
        }
    }
}
