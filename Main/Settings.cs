// System
using System;

// Internal
using VDownload.Core.Services;



namespace VDownload.Main
{
    class Settings
    {
        #region MAIN

        // VALUE OF SPECIFIED SETTINGS KEY
        public static void Get(string[] args)
        {
            string output = "";
            string key = args[1].ToLower().Trim();
            try
            {
                string value = Config.Read(key);
                output = Output.Get(@"settings\get\_main.out",
                    new()
                    {
                        key,
                        value,
                    }
                );
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"settings\get\error.out"),
                        $"\n{e.Message}",
                    }
                );
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine(output);
            }
        }


        // SET VALUE OF SPECIFIED SETTINGS KEY
        public static void Set(string[] args)
        {
            string output = "";
            string key = args[1].ToLower().Trim();
            string value = args[2];
            try
            {
                string valueOld = Config.Read(key);
                Config.Write(key, value);
                output = Output.Get(@"settings\set\_main.out",
                    new()
                    {
                        key,
                        valueOld,
                        value,
                    }
                );
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"settings\set\error.out"),
                        $"\n{e.Message}",
                    }
                );
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine(output);
            }
        }


        // RESTORE DEFAULT SETTINGS
        public static void Reset()
        {
            string output = Output.Get(@"settings\reset\_main.out");
            try
            {
                Config.Restore();
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"settings\reset\error.out"),
                        $"\n{e.Message}",
                    }
                );
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine(output);
            }
        }

        #endregion
    }
}
