using System;
using System.IO;
using System.Security;
using System.Collections.Generic;
using VDownload.Parsers;

namespace VDownload
{
    class Program
    {
        // INIT FUNCTION
        static void Main(string[] args)
        {
            // Rebuild config
            try
            {
                Config.Main.Rebuild();
            }
            catch (SecurityException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\config\rebuild\error\access_denied.out"));
                Environment.Exit(0);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\config\rebuild\error\access_denied.out"));
                Environment.Exit(0);
            }
            catch (IOException)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\config\rebuild\error\file_in_use.out"));
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\config\rebuild\error\undefined.out",
                    args: new()
                    { 
                        e.Message
                    }
                ));
                Environment.Exit(0);
            }

            // Update checking
            bool checkUpdates = true;
            if (Config.Main.ReadKey("check_updates_on_start") == "0" || (args.Length != 0 && args[0].ToLower() == "check-updates"))
            {
                checkUpdates = false;
            }
            if (checkUpdates)
            {
                try
                {
                    if (Update.Available())
                    {
                        Console.WriteLine(TerminalOutput.Get(@"output\update\available_on_start.out",
                            args: new()
                            {
                                Global.ProgramInfo.RELEASES
                            }
                        ));
                        Console.ReadLine();
                    }
                }
                catch { }
            }

            // Command switch
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "help": Help(args); break;
                    case "about": About(); break;
                    case "info": Info(args); break;
                    case "download": Download(args); break;
                    case "option-preset-new": OptionPresetNew(args); break;
                    case "option-preset-delete": OptionPresetDelete(args); break;
                    case "option-preset-list": OptionPresetList(); break;
                    case "settings-get": SettingsGet(args); break;
                    case "settings-set": SettingsSet(args); break;
                    case "settings-reset": SettingsReset(); break;
                    case "check-updates": CheckUpdates(); break;
                    default: Help(new string[1] { "help" }); break;
                }
            }
            else { Help(new string[1] { "help" }); }
        }






        // LIST OF COMMANDS, SETTINGS KEYS AND FILENAME WILDCARDS
        private static void Help(string[] args)
        {
            string output = TerminalOutput.Get(@"output\main\help\commands.out"); ;
            if (args != null || args.Length > 1)
            {
                var options = Options.Get(args[1..]);
                if (options.ContainsKey("settings"))
                {
                    output = TerminalOutput.Get(@"output\main\help\settings.out");
                }
                else if (options.ContainsKey("filename"))
                {
                    output = TerminalOutput.Get(@"output\main\help\filename.out");
                }
            }
            Console.WriteLine(output);
        }


        // INFORMATIONS ABOUT PROGRAM
        private static void About()
        {
            // Used libraries list (in string)
            string librariesUsedInApp = "";
            foreach (var e in Global.ProgramInfo.LIBRARIES)
            {
                librariesUsedInApp += String.Format("- {0} ({1})\n", e.Key, e.Value);
            }

            // Console write
            Console.WriteLine(TerminalOutput.Get(@"output\main\about\about.out",
                args: new()
                {
                    Global.ProgramInfo.VERSION,
                    Global.ProgramInfo.BUILD_ID,
                    Global.ProgramInfo.AUTHOR_NAME,
                    Global.ProgramInfo.AUTHOR_GITHUB,
                    Global.ProgramInfo.REPOSITORY,
                    librariesUsedInApp,
                    Global.ProgramInfo.DONATION_LINK,
                    Global.ProgramInfo.AUTHOR_NAME,
                    Global.ProgramInfo.PROJECT_START,
                    Global.ProgramInfo.PROJECT_END,
                }
            ));
        }




        // INFORMATIONS ABOUT VIDEO (OR PLAYLIST)
        private static void Info(string[] args)
        {
            if (args.Length > 1)
            {
                string url = args[1];
                var options = Options.Get(args[2..]);
                switch (UrlWebpage.Get(url))
                {
                    case "youtube_single": Youtube.VideoInfo(url); break;
                    case "youtube_playlist": Youtube.PlaylistInfo(url, options); break;
                    case "twitch_vod": Twitch.VodInfo(url); break;
                    default: Console.WriteLine(TerminalOutput.Get(@"output\main\info\error\wrong_site.out")); break;
                }
            }
            else
            {
                Help(new string[1] { "help" });
            }
        }


        // DOWNLOADING VIDEO (OR PLAYLIST)
        private static void Download(string[] args)
        {
            if (args.Length > 1)
            {
                // Options
                Dictionary<string, string> options = Options.Get(args[2..]);
                if (options.ContainsKey("option_preset"))
                {
                    try
                    {
                        var optionsFromPreset = OptionPresets.Get(options["option_preset"]);
                        var optionsNew = optionsFromPreset;
                        foreach (string k in options.Keys)
                        {
                            optionsNew[k] = options[k];
                        }
                        options = optionsNew;
                    }
                    catch { }
                }

                // Url switch
                string url = args[1];
                switch (UrlWebpage.Get(url))
                {
                    case "youtube_single": Youtube.VideoDownload(url, options); break;
                    case "youtube_playlist": Youtube.PlaylistDownload(url, options); break;
                    case "twitch_vod": Twitch.VodDownload(url, options); break;
                    default: Console.WriteLine(TerminalOutput.Get(@"output\main\download\error\wrong_site.out")); break;
                }
            }
            else
            {
                Help(new string[1] { "help" });
            }
        }




        // CREATE NEW OPTION PRESET
        private static void OptionPresetNew(string[] args)
        {
            if (args.Length > 2)
            {
                string name = args[1];
                var options = Options.Get(args[2..]);
                string output = TerminalOutput.Get(@"output\main\option_preset\new\new.out");
                try
                {
                    OptionPresets.New(name, options);
                }
                catch (SecurityException)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\new\error\access_denied.out");
                }
                catch (UnauthorizedAccessException)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\new\error\access_denied.out");
                }
                catch (PathTooLongException)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\new\error\path_too_long.out");
                }
                catch (Exception e)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\new\error\undefined.out",
                        args: new()
                        {
                            e.Message
                        }
                    );
                }
                Console.WriteLine(output);
            }
            else
            {
                Help(new string[1] { "help" });
            }
        }


        // DELETE OPTION PRESET
        private static void OptionPresetDelete(string[] args)
        {
            if (args.Length > 1)
            {
                string name = args[1];
                string output = TerminalOutput.Get(@"output\main\option_preset\delete\delete.out");
                try
                {
                    OptionPresets.Delete(name);
                }
                catch (UnauthorizedAccessException)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\delete\error\access_denied.out");
                }
                catch (IOException)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\delete\error\file_in_use.out");
                }
                catch (Exception e)
                {
                    output = TerminalOutput.Get(@"output\main\option_preset\delete\error\undefined.out",
                        args: new()
                        {
                            e.Message
                        }
                    );
                }
                Console.WriteLine(output);
            }
            else
            {
                Help(new string[1] { "help" });
            }
        }


        // LIST OF OPTION PRESETS
        private static void OptionPresetList()
        {
            string output;
            try
            {
                output = OptionPresets.List();
            }
            catch (SecurityException)
            {
                output = TerminalOutput.Get(@"output\main\option_preset\list\error\access_denied.out");
            }
            catch (UnauthorizedAccessException)
            {
                output = TerminalOutput.Get(@"output\main\option_preset\list\error\access_denied.out");
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\main\option_preset\list\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            if (output == "")
            {
                output = TerminalOutput.Get(@"output\main\option_preset\list\empty.out");
            }
            Console.WriteLine(output);
        }




        // VALUE OF SPECIFIED SETTINGS KEY
        private static void SettingsGet(string[] args)
        {
            if (args.Length > 1)
            {
                string output;
                string key = args[1].ToLower();
                try
                {
                    if (Config.Main.ReadAll().TryGetValue(key, out string value))
                    {
                        output = TerminalOutput.Get(@"output\main\settings\get\get.out",
                            args: new()
                            {
                                key,
                                value
                            }
                        );
                    }
                    else
                    {
                        output = TerminalOutput.Get(@"output\main\settings\get\error\key_does_not_exists.out");
                    }
                }
                catch (SecurityException)
                {
                    output = TerminalOutput.Get(@"output\config\read\error\access_denied.out");
                }
                catch (UnauthorizedAccessException)
                {
                    output = TerminalOutput.Get(@"output\config\read\error\access_denied.out");
                }
                catch (Exception e)
                {
                    output = TerminalOutput.Get(@"output\config\read\error\undefined.out",
                        args: new()
                        { 
                            e.Message
                        }
                    );
                }
                Console.WriteLine(output);
            }
            else
            {
                Help(new string[1] { "help" });
            }
        }


        // CHANGE SETTINGS
        private static void SettingsSet(string[] args)
        {
            if (args.Length > 2)
            {
                string output;
                string key = args[1].ToLower().Trim();
                string value = args[2].Trim();
                if (key.Length == 0)
                {
                    output = TerminalOutput.Get(@"output\main\settings\set\error\key_is_empty.out");
                }
                else if (!(Config.Main.ReadAll().ContainsKey(key)))
                {
                    output = TerminalOutput.Get(@"output\main\settings\set\error\key_does_not_exist.out");
                }
                else
                {
                    try
                    {
                        string oldValue = Config.Main.ReadKey(key);
                        try
                        {
                            Config.Main.Write(key, value);
                            output = TerminalOutput.Get(@"output\main\settings\set\set.out",
                                args: new()
                                {
                                    key,
                                    oldValue,
                                    value
                                }
                            );
                        }
                        catch (SecurityException)
                        {
                            output = TerminalOutput.Get(@"output\config\write\error\access_denied.out");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            output = TerminalOutput.Get(@"output\config\write\error\access_denied.out");
                        }
                        catch (IOException)
                        {
                            output = TerminalOutput.Get(@"output\config\write\error\file_in_use.out");
                        }
                        catch (Exception e)
                        {
                            output = TerminalOutput.Get(@"output\config\write\error\undefined.out",
                                args: new()
                                {
                                    e.Message
                                }
                            );
                        }
                    }
                    catch (SecurityException)
                    {
                        output = TerminalOutput.Get(@"output\config\read\error\access_denied.out");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        output = TerminalOutput.Get(@"output\config\read\error\access_denied.out");
                    }
                    catch (Exception e)
                    {
                        output = TerminalOutput.Get(@"output\config\read\error\undefined.out",
                            args: new()
                            {
                                e.Message
                            }
                        );
                    }
                }
                Console.WriteLine(output);
            }
            else
            {
                Help(new string[1] { "help" });
            }
        }


        // RESTORE DEFAULT SETTINGS
        private static void SettingsReset()
        {
            string output = TerminalOutput.Get(@"output\main\settings\reset\reset.out");
            try
            {
                Config.Main.ResetFile();
            }
            catch (SecurityException)
            {
                output = TerminalOutput.Get(@"output\config\reset\error\access_denied.out");
            }
            catch (UnauthorizedAccessException)
            {
                output = TerminalOutput.Get(@"output\config\reset\error\access_denied.out");
            }
            catch (IOException)
            {
                output = TerminalOutput.Get(@"output\config\reset\error\file_in_use.out");
            }
            catch (Exception e)
            {
                output = TerminalOutput.Get(@"output\config\reset\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                );
            }
            Console.WriteLine(output);
        }




        // CHECK UPDATES
        private static void CheckUpdates()
        {
            string output = TerminalOutput.Get(@"output\update\unavailable.out");
            try
            {
                if (Update.Available())
                {
                    output = TerminalOutput.Get(@"output\update\available.out",
                        args: new()
                        {
                            Global.ProgramInfo.RELEASES
                        }
                    );
                }
            }
            catch { }
            Console.WriteLine(output);
        }
    }
}
