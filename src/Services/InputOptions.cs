using System.Collections.Generic;

namespace VDownload.Services
{
    class InputOptions
    {
        #region CONSTANTS

        private static readonly string prefix = "--"; // OPTION PREFIX
        private static readonly string separator = "="; // KEY-VALUE SEPARATOR

        #endregion CONSTANTS





        #region MAIN

        // PARSE CONSOLE INPUT OPTIONS
        public static Dictionary<string, string> Parse(string[] args)
        {
            // Pack into dictionary
            Dictionary<string, string> options = new();
            foreach (string a in args)
            {
                // Skip if arg is only a mark
                if (a[..prefix.Length] != prefix)
                {
                    continue;
                }

                // KeyValuePair option
                if (a.Contains(separator))
                {
                    string[] kvPair = a[prefix.Length..].Split(separator, 2);
                    if (kvPair[0].Trim() == "" || kvPair[1].Trim() == "")
                    {
                        continue;
                    }
                    options[kvPair[0]] = kvPair[1];
                }

                // OnlyKey option
                else
                {
                    string key = a[prefix.Length..];
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

        // SWITCH BETWEEN CONFIG AND CONSOLE INPUT OPTIONS
        public static string Switch(string name, Dictionary<string, string> options)
        {
            // Get value from config 
            string value = Config.Main.ReadKey(name);

            // Check if option is available
            if (options.ContainsKey(name) && !(options[name] == null))
            {
                value = options[name];
            }

            // Return value
            return value;
        }

        #endregion MAIN
    }
}
