// System
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;



namespace VDownload.Core.Services
{
    class TaskID
    {
        #region CONSTANTS

        private static readonly char[] Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly string IdsFolderPath = Globals.Path.Temp;
        private static readonly Random Random = new();
        private static readonly int Length = 10;

        #endregion



        #region MAIN

        // IDS LIST
        private static readonly List<string> ID = new();


        // COMPLETE LIST WITH USED IDS FROM FOLDER
        static TaskID()
        {
            Directory.CreateDirectory(IdsFolderPath);
            string[] dirs = Directory.GetDirectories(IdsFolderPath);
            foreach (string d in dirs)
            {
                string dir = d.Replace(IdsFolderPath, "");
                if ((dir.All(char.IsDigit) || dir.All(char.IsUpper)) && dir.Length == Length && !ID.Contains(dir))
                {
                    ID.Add(dir);
                }
            }
        }


        // GET NEW ID
        public static string Get()
        {
            string id;
            while (true)
            {
                id = Generate(Length);
                if (!ID.Contains(id))
                    break;
            }
            ID.Add(id);
            return id;
        }


        // FREE ID
        public static void Free(string id)
        {
            ID.Remove(id);
        }

        #endregion



        #region INTERNAL

        // GENERATE ID
        private static string Generate(int length)
        {
            string id = "";
            

            while (id.Length < length)
            {
                id += Chars[Random.Next(0, Chars.Length)];
            }

            return id;
        }

        #endregion
    }
}
