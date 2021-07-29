// System
using System.Collections.Generic;



namespace VDownload.Core.Services
{
    class Args
    {
        #region CONSTANTS

        private static readonly string Prefix = "--"; // OPTION PREFIX
        private static readonly string Separator = "="; // KEY-VALUE OPTION SEPARATOR

        #endregion



        #region MAIN

        // PARSE ARGUMENTS
        public static Dictionary<string, string> Parse(string[] args)
        {
            // Pack into dictionary
            Dictionary<string, string> options = new();
            foreach (string a in args)
            {
                // Skip if arg is only a mark
                if (a[..Prefix.Length] != Prefix)
                {
                    continue;
                }

                // KeyValuePair option
                if (a.Contains(Separator))
                {
                    string[] kvPair = a[Prefix.Length..].Split(Separator, 2);
                    if (kvPair[0].Trim() == "" || kvPair[1].Trim() == "")
                    {
                        continue;
                    }
                    options[kvPair[0]] = kvPair[1];
                }

                // OnlyKey option
                else
                {
                    string key = a[Prefix.Length..];
                    if (key == "")
                    {
                        continue;
                    }
                    options[key] = null;
                }
            }

            // Return options
            return options;
        }

        #endregion
    }
}
