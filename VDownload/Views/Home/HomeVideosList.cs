using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Services;

namespace VDownload.Views.Home
{
    public class HomeVideosList
    {
        // VIDEO OBJECTS LIST
        public static List<HomeVideoPanel> VideoList = new List<HomeVideoPanel>();

        // WAIT IN QUEUE
        public static async Task WaitInQueue(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                while (!(VideoList.Where(video => video.VideoStatus == Core.Enums.VideoStatus.InProgress).Count() < (int)Config.GetValue("max_active_video_task")) && !token.IsCancellationRequested)
                {
                    await Task.Delay(50);
                }
            });
        }

        #region GET UNIQUE ID

        // VARIABLES
        private static readonly Random Random = new Random();
        private static readonly char[] CharsID = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly int LengthID = 10;
        private static readonly List<string> UsedIDs = new List<string>();

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
            } while (UsedIDs.Contains(id));
            UsedIDs.Add(id);
            return id;
        }

        public static void DisposeUniqueID(string id)
        {
            UsedIDs.Remove(id);
        }

        #endregion
    }
}
