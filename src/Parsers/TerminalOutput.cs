using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VDownload.Parsers
{
    class TerminalOutput
    {
        // CONSTANTS
        private static readonly string replaceChar = "{#}";
        private static readonly string outputFilesLocation = Global.Paths.MAIN;


        // MAIN
        public static string Get(string file, bool upSP = true, bool downSP = true, List<string> args = null)
        {
            // Read file
            string text = File.ReadAllText(outputFilesLocation + @"\" + file);

            // Replace replaceChars with args
            while (text.Contains(replaceChar) && args != null && args.Count > 0)
            {
                Regex r = new(replaceChar, RegexOptions.IgnoreCase);
                text = r.Replace(text, args[0], 1);
                args.RemoveAt(0);
            }

            // Up and down spaces
            if (upSP) { text = "\n" + text; }
            if (downSP) { text += "\n"; }

            return text;
        }
    }
}
