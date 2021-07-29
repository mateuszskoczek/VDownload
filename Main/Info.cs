// System
using System;
using System.Threading.Tasks;

// Internal
using VDownload.Core.Services;



namespace VDownload.Main
{
    class Info
    {
        #region MAIN

        // LIST OF COMMANDS, SETTINGS KEYS AND FILENAME WILDCARDS
        public static void Help(string[] args)
        {
            // Help type switch
            string output = Output.Get(@"help\commands.out");
            if (args != null && args.Length > 1)
            {
                Console.WriteLine(args.Length);
                var options = Args.Parse(args[1..]);
                if (options.ContainsKey("settings"))
                {
                    output = Output.Get(@"help\settings.out");
                }
                else if (options.ContainsKey("filename"))
                {
                    output = Output.Get(@"help\filename.out");
                }
            }
            Console.WriteLine();
            Console.WriteLine(output);
        }


        // INFORMATIONS ABOUT PROGRAM
        public static async Task About()
        {
            // Check updates
            string update_info = Output.Get(@"about\update_unavailable.out");
            try
            {
                if (await Update.Available())
                {
                    update_info = Output.Get(@"about\update_available.out",
                        new()
                        {
                            await Update.LatestVersion(),
                            await Update.LatestBuildID(),
                            await Update.LatestUrl()
                        }
                    );
                }
            }
            catch { }

            // Console write
            Console.WriteLine();
            Console.WriteLine(Output.Get(@"about\_main.out",
                new()
                {
                    Core.Globals.Info.Version,
                    Core.Globals.Info.Build,
                    Core.Globals.Info.Author,
                    Core.Globals.Info.AuthorGithub,
                    Core.Globals.Info.Repository,
                    Core.Globals.Info.Donation,
                    update_info
                }
            ));
        }

        #endregion
    }
}
