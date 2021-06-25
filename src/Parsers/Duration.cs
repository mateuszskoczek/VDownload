using System;

namespace VDownload.Parsers
{
    class Duration
    {
        public static string ParseSeconds(int seconds)
        {
            // Duration object
            var durationObj = TimeSpan.FromSeconds(seconds);

            // Output
            string output = "";
            if (durationObj.Days > 0)
            {
                output += String.Format("{0}.", durationObj.Days);
            }
            output += String.Format("{0}:{1}:{2}", durationObj.Hours, durationObj.Minutes, durationObj.Seconds);

            return output;
        }
    }
}
