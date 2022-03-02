using System;
using System.Collections.Generic;

namespace VDownload.Core.Services
{
    public class TaskId
    {
        #region CONSTANTS

        // RANDOM
        private static readonly Random Random = new Random();

        // ID SETTINGS
        private static readonly char[] IDChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly int IDLength = 10;

        #endregion



        #region PROPERTIES

        // USED IDS LIST
        private static readonly List<string> UsedIDs = new List<string>();

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
                    id += IDChars[Random.Next(0, IDChars.Length)];
                }
            } while (UsedIDs.Contains(id));
            UsedIDs.Add(id);
            return id;
        }

        // DISPOSE TASK ID
        public static void Dispose(string id)
        {
            UsedIDs.Remove(id);
        }

        #endregion
    }
}
