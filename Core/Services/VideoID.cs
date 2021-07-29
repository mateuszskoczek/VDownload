using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Services
{
    class VideoID
    {
        #region CONSTANTS

        private static readonly char[] Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly Random Random = new();
        private static readonly int Length = 10;

        #endregion



        #region MAIN

        private static List<string> ID = new();

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

        public static void Free(string id)
        {
            ID.Remove(id);
        }

        #endregion



        #region INTERNAL

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
