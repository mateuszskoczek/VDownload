using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Converters
{
    public class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean)
            {
                return boolean.ToString();
            }
            return bool.FalseString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string str)
            {
                return str == bool.TrueString;
            }
            return false;
        }
    }
}
