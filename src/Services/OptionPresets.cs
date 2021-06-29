using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace VDownload.Services
{
    class OptionPresets
    {
        #region CONSTANTS

        private static readonly string presetsPath = String.Format(@"{0}\presets\", Global.Paths.APPDATA); // PATH TO OPTION PRESETS FOLDER
        private static readonly string separator = " = "; // SEPARATOR CHAR IN OPTION PRESET FILE
        private static readonly string fileExtension = "opp"; // OPTION PRESET FILE EXTENSION

        #endregion CONSTANTS



        #region MANAGE

        // CREATE NEW OPTION PRESET
        public static void New(string name, Dictionary<string, string> options)
        {
            // Create presets directory if not exists
            Directory.CreateDirectory(presetsPath);

            // Filter filename
            name = Filename.Process(name, new());

            // Preset path
            string path = String.Format(@"{0}\{1}.{2}", presetsPath, name, fileExtension);

            // Write to file
            List<string> lines = new();
            foreach (string k in options.Keys)
            {
                if (options[k] == null)
                {
                    lines.Add(k);
                }
                else
                {
                    lines.Add(String.Format("{0}{1}{2}", k, separator, options[k]));
                }
            }
            File.WriteAllLines(path, lines);
        }

        // DELETE OPTION PRESET
        public static void Delete(string name)
        {
            // Filter filename
            name = Filename.Process(name, new());

            // Delete file
            File.Delete(String.Format(@"{0}\{1}.{2}", presetsPath, name, fileExtension));
        }

        // GET OPTIONS FROM OPTION PRESET
        public static Dictionary<string, string> Get(string name)
        {
            // Read file
            var fileContent = File.ReadLines(String.Format(@"{0}\{1}.{2}", presetsPath, name, fileExtension));

            // Pack into dictionary
            Dictionary<string, string> options = new();
            foreach (string l in fileContent)
            {
                string[] keyValue = l.Split(separator);
                if (keyValue.Length == 1 && keyValue[0].Trim() != "")
                {
                    options.Add(keyValue[0], null);
                }
                else if (keyValue.Length > 1 && keyValue[0].Trim() != "" && keyValue[1].Trim() != "")
                {
                    options.Add(keyValue[0], keyValue[1]);
                }
            }

            // Return options
            return options;
        }

        #endregion MANAGE



        #region OTHER

        // LIST OF OPTION PRESETS
        public static string List()
        {
            // Get presets list
            var presets = from f in Directory.EnumerateFiles(presetsPath) select f.Replace(presetsPath, "");

            // Build list
            string output = "";
            int i = 1;
            foreach (string p in presets)
            {
                if (p.Split('.')[^1] != fileExtension) // Skip other files
                {
                    continue;
                }
                else
                {
                    // Write name
                    string name = p.Replace(String.Format(".{0}", fileExtension), "");
                    output += String.Format("{0}. {1}\n", i, name);

                    // Get options
                    var options = Get(name);

                    // Write options
                    foreach (string k in options.Keys)
                    {
                        if (options[k] == null)
                        {
                            output += String.Format("\t--{0}", k);
                        }
                        else
                        {
                            output += String.Format("\t--{0}=\"{1}\"", k, options[k]);
                        }
                        output += "\n";
                    }
                }
                i++;
            }

            // Return list
            return output;
        }

        #endregion OTHER
    }
}
