using System;
using System.Collections.Generic;

namespace VDownload.Core.Services
{
    public class TaskId
    {
        #region CONSTANTS

        // ID SETTINGS
        private static readonly char[] IDChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly int IDLength = 10;

        // IDS LIST
        private static readonly List<string> IDList = new List<string>();

        #endregion



        #region METHODS

        // GET TASK ID
        public static string Get()
        {
            string id;
            do
            {
                id = "";
                while (id.Length < IDLength)
                {
                    id += IDChars[new Random().Next(0, IDChars.Length)];
                }
            } while (IDList.Contains(id));
            IDList.Add(id);
            return id;
        }

        // DISPOSE TASK ID
        public static void Dispose(string id)
        {
            IDList.Remove(id);
        }

        #endregion
    }
}
