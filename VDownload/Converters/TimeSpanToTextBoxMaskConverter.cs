using System;
using Windows.UI.Xaml.Data;

namespace VDownload.Converters
{
    public class TimeSpanToTextBoxMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string mask = string.Empty;

            if (Math.Floor(((TimeSpan)value).TotalHours) > 0) mask += $"{Math.Floor(((TimeSpan)value).TotalHours).ToString()[0]}{new string('9', Math.Floor(((TimeSpan)value).TotalHours).ToString().Length - 1)}:";
            if (Math.Floor(((TimeSpan)value).TotalMinutes) > 0) mask += Math.Floor(((TimeSpan)value).TotalHours) > 0 ? $"59:" : $"{((TimeSpan)value).Minutes.ToString()[0]}{new string('9', ((TimeSpan)value).Minutes.ToString().Length - 1)}:";
            mask += Math.Floor(((TimeSpan)value).TotalMinutes) > 0 ? $"59" : $"{((TimeSpan)value).Seconds.ToString()[0]}{new string('9', ((TimeSpan)value).Seconds.ToString().Length - 1)}";
            
            return mask;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
