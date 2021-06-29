using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services
{
    class StringC
    {
        // TRIM STRING TO LENGTH
        public static string TrimToLength(string str, int length)
        {
            // Get length
            int strLength = str.Length;

            string output;
            if (strLength < length)
            {
                output = str;
            }
            else
            {
                output = String.Format("{0}{1}", str[..(length-3)], "...");
            }

            return output;
        }
    }
}
