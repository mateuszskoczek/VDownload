using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Services
{
    public class TimeSpanCustomFormat
    {
        // (TH:)MM:SS
        public static string ToOptTHBaseMMSS(TimeSpan timeSpan, params TimeSpan[] formatBase)
        {
            string formattedTimeSpan = string.Empty;

            int maxTHLength = 0;
            if (Math.Floor(timeSpan.TotalHours) > 0)
            {
                maxTHLength = Math.Floor(timeSpan.TotalHours).ToString().Length;
                foreach (TimeSpan format in formatBase)
                {
                    int THLength = Math.Floor(format.TotalHours) > 0 ? Math.Floor(timeSpan.TotalHours).ToString().Length : 0;
                    if (THLength > maxTHLength) maxTHLength = THLength;
                }
                formattedTimeSpan += $"{((int)Math.Floor(timeSpan.TotalHours)).ToString($"D{maxTHLength}")}:";
            }
            formattedTimeSpan += maxTHLength == 0 ? $"{timeSpan.Minutes}:" : $"{timeSpan.Minutes:00}:";
            formattedTimeSpan += $"{timeSpan.Seconds:00}";

            return formattedTimeSpan;
        }

        // ((TH:)MM:)SS
        public static string ToOptTHMMBaseSS(TimeSpan timeSpan, params TimeSpan[] formatBase)
        {
            string formattedTimeSpan = string.Empty;

            int maxTHLength = 0;
            if (Math.Floor(timeSpan.TotalHours) > 0)
            {
                maxTHLength = Math.Floor(timeSpan.TotalHours).ToString().Length;
                foreach (TimeSpan format in formatBase)
                {
                    int THLength = Math.Floor(format.TotalHours) > 0 ? Math.Floor(timeSpan.TotalHours).ToString().Length : 0;
                    if (THLength > maxTHLength) maxTHLength = THLength;
                }
                formattedTimeSpan += $"{((int)Math.Floor(timeSpan.TotalHours)).ToString($"D{maxTHLength}")}:";
            }
            bool MM = false;
            if (Math.Floor(timeSpan.TotalMinutes) > 0)
            {
                formattedTimeSpan += maxTHLength > 0 ? $"{timeSpan.Minutes:00}:" : $"{timeSpan.Minutes}:";
                MM = true;
            }
            formattedTimeSpan += MM ? $"{timeSpan.Seconds:00}:" : $"{timeSpan.Seconds}:";

            return formattedTimeSpan;
        }
    }
}
