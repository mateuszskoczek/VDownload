// System
using System.IO;
using System.Linq;
using System.Collections.Generic;



namespace VDownload.Core.Services
{
    class OptionPreset
    {
        #region CONSTANTS

        private static readonly string PresetsPath = $@"{Globals.Path.AppData}\presets\"; // OPTION PRESETS FOLDER PATH
        private static readonly string Separator = " = "; // SEPARATOR CHARACTER IN OPTION PRESET FILE
        private static readonly string FileExtension = "opp"; // OPTION PRESET FILE EXTENSION

        #endregion



        #region MAIN

        // CREATE NEW OPTION PRESET
        public static void New(string name, Dictionary<string, string> options)
        {
            // Create option presets directory if not exists
            Directory.CreateDirectory(PresetsPath);

            // Filter filename
            name = Filename.Process(name, new());

            // Preset path
            string path = $@"{PresetsPath}\{name}.{FileExtension}";

            // Write to file
            List<string> lines = new();
            foreach (var o in options)
            {
                if (options[o.Key] == null)
                {
                    lines.Add(o.Key);
                }
                else
                {
                    lines.Add($"{o.Key}{Separator}{o.Value}");
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
            File.Delete($@"{PresetsPath}\{name}.{FileExtension}");
        }


        // GET LIST OF OPTION PRESET
        public static string[] GetList()
        {
            // Get presets list
            var presets = from f in Directory.EnumerateFiles(PresetsPath) where f.Split('.')[^1] == FileExtension select f.Replace(PresetsPath, "").Replace($".{FileExtension}", "");

            // Return list
            return presets.ToArray();
        }


        // GET OPTIONS FROM OPTION PRESET
        public static Dictionary<string, string> GetOptions(string name)
        {
            // Read file
            var fileContent = File.ReadLines($@"{PresetsPath}\{name}.{FileExtension}");

            // Pack into dictionary
            Dictionary<string, string> options = new();
            foreach (string l in fileContent)
            {
                string[] keyValue = l.Split(Separator);
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

        #endregion
    }
}
