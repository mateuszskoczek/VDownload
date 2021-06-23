using System;
using System.Collections.Generic;
using Octokit;
using VDownload.Parsers;

namespace VDownload
{
    class Program
    {
        // Init function
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                bool checkUpdates = true;
                if (Config.Main.ReadKey("check_updates_on_start") == "0" || args[0].ToLower() == "check-updates")
                {
                    checkUpdates = false;
                }
                if (checkUpdates)
                {
                    try
                    {
                        var githubClient = new GitHubClient(new ProductHeaderValue("VDownload"));
                        var latestRelease = githubClient.Repository.Release.GetAll("mateuszskoczek", "VDownload").Result[0];
                        float latestReleaseBuildID = float.Parse(latestRelease.Name.Split(' ')[1].Replace("(", "").Replace(")", "").Replace('.', ','));
                        if (latestReleaseBuildID > float.Parse(Global.ProgramInfo.BUILD_ID.Replace('.', ',')))
                        {
                            Console.WriteLine(TerminalOutput.Get(@"output\main\update_available_on_start.out", args: new() { latestRelease.HtmlUrl }));
                            Console.ReadLine();
                        }
                    }
                    catch { }
                }
                switch (args[0].ToLower())
                {
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
                    default: Help(); break;
                }
            }
            else { Help(); }
        }


        // Commands and settings keys list
        private static void Help()
        {
            Console.WriteLine(TerminalOutput.Get(
                file: @"output\main\help.out"
            ));
        }


        // Informations about program
        private static void About()
        {
            string librariesUsedInApp = "";
            foreach (var e in Global.ProgramInfo.LIBRARIES)
            {
                librariesUsedInApp += String.Format("- {0} ({1})\n", e.Key, e.Value);
            }
            Console.WriteLine(TerminalOutput.Get(
                file: @"output\main\about.out",
                args: new()
                {
                    Global.ProgramInfo.VERSION,
                    Global.ProgramInfo.BUILD_ID,
                    Global.ProgramInfo.AUTHOR_NAME,
                    Global.ProgramInfo.AUTHOR_GITHUB,
                    Global.ProgramInfo.REPOSITORY,
                    librariesUsedInApp.TrimEnd(),
                    Global.ProgramInfo.DONATION_LINK,
                    Global.ProgramInfo.AUTHOR_NAME,
                    Global.ProgramInfo.PROJECT_START,
                    Global.ProgramInfo.PROJECT_END,
                }
            ));
        }


        // Informations about specified video (or playlist)
        private static void Info(string[] args)
        {
            if (args.Length < 2)
            {
                Help();
            }
            else
            {
                string url = args[1];
                var options = Options.Get(args[2..]);
                switch (UrlWebpage.Get(url))
                {
                    case "youtube_single": Youtube.VideoInfo(url); break;
                    case "youtube_playlist": Youtube.PlaylistInfo(url, options); break;
                    default: Console.WriteLine(TerminalOutput.Get(@"output\main\error_wrong_site.out")); break;
                }
            }
        }


        // Downloading video (or playlist)
        private static void Download(string[] args)
        {
            if (args.Length < 2)
            {
                Help();
            }
            else
            {
                string url = args[1];
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
                switch (UrlWebpage.Get(url))
                {
                    case "youtube_single": Youtube.VideoDownload(url, options); break;
                    case "youtube_playlist": Youtube.PlaylistDownload(url, options); break;
                    default: Console.WriteLine(TerminalOutput.Get(@"output\main\error_wrong_site.out")); break;
                }
            }
        }


        private static void OptionPresetNew(string[] args)
        {
            if (args.Length < 3)
            {
                Help();
            }
            else
            {
                string name = args[1];
                var options = Options.Get(args[2..]);
                string output = TerminalOutput.Get(@"output\main\option_preset_new.out");
                try
                {
                    OptionPresets.New(name, options);
                }
                catch
                {
                    output = TerminalOutput.Get(@"output\main\error_option_preset_cannot_be_created.out");
                }
                Console.WriteLine(output);
            }
        }


        private static void OptionPresetDelete(string[] args)
        {
            if (args.Length < 2)
            {
                Help();
            }
            else
            {
                string name = args[1];
                string output = TerminalOutput.Get(@"output\main\option_preset_delete.out");
                try
                {
                    OptionPresets.Delete(name);
                }
                catch
                {
                    output = TerminalOutput.Get(@"output\main\error_option_preset_cannot_be_deleted.out");
                }
                Console.WriteLine(output);
            }
        }


        private static void OptionPresetList()
        {
            string output;
            try
            {
                output = OptionPresets.List();
            }
            catch
            {
                output = TerminalOutput.Get(@"output\main\error_while_getting_presets_list.out");
            }
            if (output == "")
            {
                output = TerminalOutput.Get(@"output\main\option_preset_list_empty.out");
            }
            Console.WriteLine(output);
        }


        // Returns value of specified settings key
        private static void SettingsGet(string[] args)
        {
            if (args.Length < 2)
            {
                Help();
            }
            else
            {
                string output;
                string key = args[1].ToLower();
                if (Config.Main.ReadAll().TryGetValue(key, out string value))
                {
                    output = TerminalOutput.Get(
                        file: @"output\main\get_settings.out",
                        args: new()
                        {
                            key,
                            value
                        }
                    );
                }
                else
                {
                    output = TerminalOutput.Get(@"output\main\error_key_does_not_exists.out");
                }
                Console.WriteLine(output);
            }
        }


        // Sets value of specified settings key
        private static void SettingsSet(string[] args)
        {
            if (args.Length < 3)
            {
                Help();
            }
            else
            {
                string output;
                string key = args[1].ToLower();
                string value = args[2];
                if (key.Trim() == "")
                {
                    output = TerminalOutput.Get(@"output\main\error_key_is_an_empty_string.out");
                }
                else if (value.Trim() == "")
                {
                    output = TerminalOutput.Get(@"output\main\error_value_is_an_empty_string.out");
                }
                else if (!(Config.Main.ReadAll().ContainsKey(key)))
                {
                    output = TerminalOutput.Get(@"output\main\error_key_does_not_exists.out");
                }
                else
                {
                    string oldValue = Config.Main.ReadKey(key);
                    Config.Main.Write(key, value);
                    output = TerminalOutput.Get(
                        file: @"output\main\set_settings.out", 
                        args: new()
                        {
                            key,
                            oldValue,
                            value
                        }
                    );
                }
                Console.WriteLine(output);
            }
        }


        // Reset (delete) configuration file
        private static void SettingsReset()
        {
            string output = TerminalOutput.Get(@"output\main\reset_settings.out");
            try
            {
                Config.Main.ResetFile();
            }
            catch
            {
                output = TerminalOutput.Get(@"output\main\error_default_settings_cannot_be_restored.out");
            }
            Console.WriteLine(output);
        }


        // Check updates
        private static void CheckUpdates()
        {
            string output = TerminalOutput.Get(@"output\main\update_unavailable.out");
            try
            {
                var githubClient = new GitHubClient(new ProductHeaderValue("VDownload"));
                var latestRelease = githubClient.Repository.Release.GetAll("mateuszskoczek", "VDownload").Result[0];
                string latestReleaseBuildID = latestRelease.Name.Split(' ')[1].Replace("(", "").Replace(")", "");
                if (float.Parse(latestReleaseBuildID.Replace('.', ',')) > float.Parse(Global.ProgramInfo.BUILD_ID.Replace('.', ',')))
                {
                    output = TerminalOutput.Get(
                        @"output\main\update_available.out",
                        args: new()
                        {
                            latestRelease.HtmlUrl,
                            Global.ProgramInfo.VERSION,
                            latestRelease.Name.Split(' ')[0],
                        }
                    );
                }
            }
            catch { }
            Console.WriteLine(output);
        }
    }
}
