using System.Collections.Generic;

namespace VDownload.Parsers
{
    class Options
    {
        private static readonly string mark = "--";
        private static readonly char separator = '=';
        public static Dictionary<string, string> Get(string[] args)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            foreach (string a in args)
            {
                if (a[..mark.Length] != mark)
                {
                    continue;
                }
                if (a.Contains(separator))
                {
                    string[] kvPair = a[mark.Length..].Split(new[] { separator }, 2);
                    if (kvPair[0].Trim() == "" || kvPair[1].Trim() == "")
                    {
                        continue;
                    }
                    options[kvPair[0]] = kvPair[1];
                }
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
