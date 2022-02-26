using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace VDownload.Converters
{
    public class TimeSpanToTextBoxMaskElementsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            HashSet<int> maskElements = new HashSet<int>();

            if (Math.Floor(((TimeSpan)value).TotalHours) > 0) maskElements.Add(int.Parse(Math.Floor(((TimeSpan)value).TotalHours).ToString()[0].ToString()));
            if (Math.Floor(((TimeSpan)value).TotalMinutes) > 0)
            {
                if (Math.Floor(((TimeSpan)value).TotalHours) > 0) maskElements.Add(5);
                else maskElements.Add(int.Parse(((TimeSpan)value).Minutes.ToString()[0].ToString()));
            }
            if (Math.Floor(((TimeSpan)value).TotalMinutes) > 0) maskElements.Add(5);
            else maskElements.Add(int.Parse(((TimeSpan)value).Seconds.ToString()[0].ToString()));

            List<string> maskElementsString = new List<string>();
            foreach (int i in maskElements)
            {
                if (i != 9) maskElementsString.Add($"{i}:[0-{i}]");
            }

            return string.Join(',', maskElementsString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
