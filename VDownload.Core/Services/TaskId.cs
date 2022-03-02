using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Services
{
    public class TaskId
    {
        // VARIABLES
        private static readonly Random Random = new Random();
        private static readonly char[] CharsID = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly int LengthID = 10;
        private static readonly List<string> UsedIDs = new List<string>();

        // METHOD
        public static string Get()
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

        public static void Dispose(string id)
        {
            UsedIDs.Remove(id);
        }
    }
}
