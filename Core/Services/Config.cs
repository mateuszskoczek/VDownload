// System
using System;
using System.IO;
using System.Collections.Generic;



namespace VDownload.Core.Services
{
    class Config
    {
        #region CONSTANTS

        private static readonly string Path = $@"{Globals.Path.AppData}\config.cfg"; // CONFIGURATION FILE PATH
        private static readonly string Separator = " = "; // CONFIGURATION FILE SEPARATOR
        private static readonly Dictionary<string, string> DefaultData = new() // DEFAULT CONFIGURATION FILE CONTENT
        {
            { "ffmpeg_executables_path", $@"{Globals.Path.Main}\ffmpeg\" },
            { "temp_download_path", Globals.Path.Temp },
            { "temp_ffmpeg_path", $@"{Globals.Path.Temp}\ffmpeg\" },
            { "max_downloaded_videos", "0" },
            { "max_downloaded_chunks", "0" },
            { "default_output_path", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },
            { "default_output_filename", "%title%" },
            { "default_output_vext", "mp4" },
            { "default_output_aext", "mp3" },
            { "date_format", "yyyy.MM.dd" },
        };

        #endregion



        #region MAIN

        // READ DATA
        public static string Read(string key)
        {
            // Get data from configuration file
            var data = ReadData();

            // Return value
            if (data.TryGetValue(key, out string value))
            {
                return value;
            }
            else
            {
                throw new InvalidConfigKeyException();
            }
        }


        // WRITE DATA
        public static void Write(string key, string value)
        {
            // Check if key is valid
            if (!DefaultData.ContainsKey(key))
            {
                throw new InvalidConfigKeyException();
            }

            // Get configuration file data
            var data = ReadData();

            // Write data to file
            data[key] = value;
            WriteData(data);
        }


        // REBUILD DATA FROM DEFAULT
        public static void Rebuild()
        {
            // Init with default data
            var rebuildedConfig = DefaultData;

            // Swap default data with existing data
            var oldConfig = ReadData();
            foreach (var e in oldConfig)
            {
                rebuildedConfig[e.Key] = e.Value;
            }

            // Write new data
            WriteData(rebuildedConfig);

            /*
             *  Exceptions:
             *  - IOException
             *  - UnauthorizedAccessException
             *  - PathTooLongException
             *  - DirectoryNotFoundException
             *  - SecurityException
             */
        }


        // RESTORE FACTORY DATA
        public static void Restore()
        {
            // Delete old file
            File.Delete(Path);

            // Write new data
            WriteData(DefaultData);
        }

        #endregion



        #region OTHER

        // READ DATA TO FILE
        private static Dictionary<string, string> ReadData()
        {
            // Create file if does not exist
            if (!File.Exists(Path))
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                File.Create(Path).Close();
                WriteData(DefaultData);
            }

            // Pack data into dictionary
            Dictionary<string, string> data = new();
            foreach (string l in File.ReadAllLines(Path))
            {
                try
                {
                    string[] keyValue = l.Split(Separator);
                    data[keyValue[0]] = String.Join(Separator, keyValue[1..]);
                }
                catch
                {
                    continue;
                }
            }

            // Return data
            return data;
        }


        // WRITE DATA TO FILE
        private static void WriteData(Dictionary<string, string> data)
        {
            // Pack data into list
            List<string> lines = new();
            foreach (var e in data)
            {
                lines.Add(e.Key + Separator + e.Value);
            }

            // Write
            File.WriteAllLines(Path, lines);
        }

        #endregion
    }

    class InvalidConfigKeyException : Exception
    {
        public InvalidConfigKeyException()
        {
        }

        public InvalidConfigKeyException(string message)
            : base(message)
        {
        }

        public InvalidConfigKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
