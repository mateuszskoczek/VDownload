using System.Collections.Generic;

namespace VDownload.Parsers
{
    class Options
    {
        // CONSTANTS
        private static readonly string mark = "--";
        private static readonly string separator = "=";


        // MAIN
        public static Dictionary<string, string> Get(string[] args)
        {
            Dictionary<string, string> options = new();
            foreach (string a in args)
            {
                // Skip if arg is only a mark
                if (a[..mark.Length] != mark)
                {
                    continue;
                }

                // KeyValuePair option
                if (a.Contains(separator))
                {
                    string[] kvPair = a[mark.Length..].Split(separator, 2);
                    if (kvPair[0].Trim() == "" || kvPair[1].Trim() == "")
                    {
                        continue;
                    }
                    options[kvPair[0]] = kvPair[1];
                }

                // OnlyKey option
                else
                {
                    string key = a[mark.Length..];
                    if (key == "")
                    {
                        continue;
                    }
                    options[key] = null;
                }
            }

            return options;
        }
    }
}
