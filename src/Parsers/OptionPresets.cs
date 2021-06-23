using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace VDownload.Parsers
{
    class OptionPresets
    {
        private static string presetsPath = String.Format(@"{0}\presets\", Global.Paths.APPDATA);
        private static string separator = " = ";

        public static void New(string name, Dictionary<string, string> options)
        {
            Directory.CreateDirectory(presetsPath);
            name = Filename.Get(name, new());
            string path = String.Format(@"{0}\{1}.opp", presetsPath, name);
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

        public static void Delete(string name)
        {
            name = Filename.Get(name, new());
            File.Delete(String.Format(@"{0}\{1}.opp", presetsPath, name));
        }

        public static Dictionary<string, string> Get(string name)
        {
            Dictionary<string, string> options = new();
            string path = String.Format(@"{0}\{1}.opp", presetsPath, name);
            var fileContent = File.ReadLines(path);
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
            return options;
        }

        public static string List()
        {
            string output = "";
            var presets = from f in Directory.EnumerateFiles(presetsPath) select f.Replace(presetsPath, "");
            int i = 1;
            foreach (string p in presets)
            {
                if (p.Split('.')[^1] != "opp")
                {
                    continue;
                }
                else
                {
                    string name = p.Replace(".opp", "");
                    output += String.Format("{0}. {1}\n", i, name);
                    var options = Get(name);
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
            return output;
        }
    }
}
