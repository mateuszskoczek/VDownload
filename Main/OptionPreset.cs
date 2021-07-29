// System
using System;

// Internal
using VDownload.Core.Services;



namespace VDownload.Main
{
    class OptionPreset
    {
        #region MAIN

        // CREATE NEW OPTION PRESET
        public static void New(string[] args)
        {
            string name = args[2];
            var options = Args.Parse(args[3..]);
            string output = Output.Get(@"option_preset\new\_main.out");
            try
            {
                Core.Services.OptionPreset.New(name, options);
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"option_preset\new\error.out"),
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


        // DELETE OPTION PRESET
        public static void Delete(string[] args)
        {
            string name = args[3];
            string output = Output.Get(@"option_preset\delete\_main.out");
            try
            {
                Core.Services.OptionPreset.Delete(name);
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"option_preset\delete\error.out"),
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


        // LIST OF OPTION PRESETS
        public static void List()
        {
            string output = "";
            try
            {
                // Get list of presets
                var presets = Core.Services.OptionPreset.GetList();

                if (presets.Length == 0)
                {
                    // Empty list
                    output = Output.Get(@"option_preset\list\empty.out");
                }
                else
                {
                    // Build list
                    int i = 1;
                    foreach (string p in presets)
                    {
                        // Name
                        output += String.Format("{0}. {1}\n", i, p);

                        // Options
                        var options = Core.Services.OptionPreset.GetOptions(p);
                        foreach (var o in options)
                        {
                            if (o.Value == null)
                            {
                                output += String.Format("\t--{0}", o.Key);
                            }
                            else
                            {
                                output += String.Format("\t--{0}=\"{1}\"", o.Key, o.Value);
                            }
                            output += "\n";
                        }
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                output = Output.Get(@"_global\error.out",
                    new()
                    {
                        Output.Get(@"option_preset\list\error.out"),
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
