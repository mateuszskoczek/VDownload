// Internal
using VDownload.Sources;

// System
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace VDownload.Services
{
    internal class Videos
    {
        #region MAIN

        // VIDEO OBJECTS LIST
        public static List<VObject> VideoObjectsList = new List<VObject>();

        // ACTIVE VIDEO TASKS LIST
        public static List<Task> VideoTasksList = new List<Task>();

        // WAIT FOR FREE SPACE IN ACTIVE VIDEO TASKS LIST
        public static async Task WaitForFreeSpace(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (!(VideoTasksList.Count < int.Parse((string)Config.GetValue("max_video_tasks"))) && !token.IsCancellationRequested)
                {
                    await Task.Delay(50);
                }
            });
        }

        #endregion



        #region GET UNIQUE ID

        // VARIABLES
        private static readonly Random Random = new Random();
        private static readonly char[] CharsID = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly int LengthID = 10;
        private static readonly List<string> UsedID = new List<string>();

        // METHOD
        public static string GetUniqueID()
        {
            string id;
            do
            {
                id = "";
                while (id.Length < LengthID)
                {
                    id += CharsID[Random.Next(0, CharsID.Length)];
                }
            } while (UsedID.Contains(id));
            UsedID.Add(id);
            return id;
        }

        #endregion
    }
}
