using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Services
{
    class StringC
    {
        public static string Cut(string str, int length)
        {
            string output = "";
            foreach (char c in str)
            {
                if (output.Length + 3 == length)
                {
                    return output + "...";
                }
                else
                {
                    output += c;
                }
            }
            return output;
        }
    }
}
