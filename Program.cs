// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;

// Internal
using VDownload.Core.Services;



namespace VDownload
{
    class Program
    {
        #region MAIN

        static async Task Main(string[] args)
        {
            // Update info
            bool updateAvailable;
            try
            {
                updateAvailable = await Update.Available();
            }
            catch
            {
                updateAvailable = false;
            }
            string updateInfo = "";
            if (updateAvailable)
            {
                updateInfo = Output.Get(@"main\start_message\update_info.out", new()
                    {
                        await Update.LatestVersion()
                    }
                );
            }

            // Start message
            Console.WriteLine(Output.Get(@"main\start_message\_main.out", new()
                {
                    Core.Globals.Info.Version,
                    updateInfo,
                    Core.Globals.Info.Author,
                    Core.Globals.Info.CopyrightSince,
                    Core.Globals.Info.CopyrightTo,
                }
            ));

            // Rebuild config
            try
            {
                Config.Rebuild();
            }
            catch (Exception e)
            {
                Console.WriteLine(Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"main\config_rebuild\error.out"),
                        $"\n{e.Message}",
                    }
                ));
                Environment.Exit(0);
            }

            // Command switch
            if (args.Length == 0)
            {
                VDownload.Main.Info.Help(args);
            }
            else
            {
                switch (args[0])
                {
                    case "help":
                    {
                        VDownload.Main.Info.Help(args);
                        break;
                    }
                    case "about":
                    {
                        await VDownload.Main.Info.About();
                        break;
                    }
                    case "info":
                    {
                        if (args.Length >= 2)
                        {
                            await VDownload.Main.Media.Info.Switch(args);
                        }
                        else
                        {
                            VDownload.Main.Info.Help(args);
                        }
                        break;
                    }
                    case "download":
                    {
                        if (args.Length >= 2)
                        {
                            await VDownload.Main.Media.Download.Switch(args);
                        }
                        else
                        {
                            VDownload.Main.Info.Help(args);
                        }
                        break;
                    }
                    case "option-preset":
                    {
                        if (args.Length > 3 && args[1] == "new")
                        {
                            VDownload.Main.OptionPreset.New(args);
                        }
                        else if (args.Length >= 3 && args[1] == "delete")
                        {
                            VDownload.Main.OptionPreset.Delete(args);
                        }
                        else
                        {
                            VDownload.Main.OptionPreset.List();
                        }
                        break;
                    }
                    case "settings":
                    {
                        if (args.Length >= 2 && args[1] == "reset")
                        {
                            VDownload.Main.Settings.Reset();
                        }
                        else if (args.Length == 2)
                        {
                            VDownload.Main.Settings.Get(args);
                        }
                        else if (args.Length >= 3)
                        {
                            VDownload.Main.Settings.Set(args);
                        }
                        else
                        {
                            VDownload.Main.Info.Help(args);
                        }
                        break;
                    }
                    default:
                    {
                        VDownload.Main.Info.Help(args);
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
