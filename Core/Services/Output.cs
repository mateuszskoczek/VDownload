// System
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace VDownload.Core.Services
{
    class Output
    {
        #region CONSTANTS

        private static readonly string ReplaceCharacter = "{#}"; // CHARACTER/STRING WHICH SHOULD BE REPLACED WITH ARGS
        private static readonly string Path = $@"{Globals.Path.Main}\output\"; // LOCALIZATION OF OUTPUT FILES

        #endregion



        #region MAIN

        // GET TEXT FROM FILE
        public static string Get(string file, List<string> args = null)
        {
            // Read file
            string text = File.ReadAllText($@"{Path}\{file}");

            // Replace replaceChars with args
            while (text.Contains(ReplaceCharacter) && args != null && args.Count > 0)
            {
                Regex r = new(ReplaceCharacter, RegexOptions.IgnoreCase);
                text = r.Replace(text, args[0], 1);
                args.RemoveAt(0);
            }

            // Return text
            return text;
        }

        #endregion
    }
}
