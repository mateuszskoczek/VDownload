using System;
using System.Linq;
using System.Text;

namespace VDownload.Core.Extensions
{
    public static class TimeSpanExtension
    {
        // To string (TH:)MM:SS
        public static string ToStringOptTHBaseMMSS(this TimeSpan timeSpan, params TimeSpan[] formatBase)
        {
            StringBuilder formattedTimeSpan = new StringBuilder();

            int maxTHLength = 0;
            foreach (TimeSpan format in formatBase.Concat(new TimeSpan[] { timeSpan }))
            {
                int THLength = Math.Floor(format.TotalHours) > 0 ? Math.Floor(timeSpan.TotalHours).ToString().Length : 0;
                if (THLength > maxTHLength)
                {
                    maxTHLength = THLength;
                }
            }
            if (maxTHLength > 0)
            {
                formattedTimeSpan.Append($"{((int)Math.Floor(timeSpan.TotalHours)).ToString($"D{maxTHLength}")}:");
            }

            formattedTimeSpan.Append(maxTHLength == 0 ? $"{timeSpan.Minutes}:" : $"{timeSpan.Minutes:00}:");

            formattedTimeSpan.Append($"{timeSpan.Seconds:00}");

            return formattedTimeSpan.ToString();
        }

        // To string ((TH:)MM:)SS
        public static string ToStringOptTHMMBaseSS(this TimeSpan timeSpan, params TimeSpan[] formatBase)
        {
            StringBuilder formattedTimeSpan = new StringBuilder();

            int maxTHLength = 0;
            foreach (TimeSpan format in formatBase.Concat(new TimeSpan[] { timeSpan }))
            {
                int THLength = Math.Floor(format.TotalHours) > 0 ? Math.Floor(timeSpan.TotalHours).ToString().Length : 0;
                if (THLength > maxTHLength)
                {
                    maxTHLength = THLength;
                }
            }
            if (maxTHLength > 0)
            {
                formattedTimeSpan.Append($"{((int)Math.Floor(timeSpan.TotalHours)).ToString($"D{maxTHLength}")}:");
            }

            bool MM = false;
            if (Math.Floor(timeSpan.TotalMinutes) > 0 || maxTHLength > 0)
            {
                formattedTimeSpan.Append(maxTHLength > 0 ? $"{timeSpan.Minutes:00}:" : $"{timeSpan.Minutes}:");
                MM = true;
            }

            formattedTimeSpan.Append(MM ? $"{timeSpan.Seconds:00}" : $"{timeSpan.Seconds}");

            return formattedTimeSpan.ToString();
        }
    }
}
