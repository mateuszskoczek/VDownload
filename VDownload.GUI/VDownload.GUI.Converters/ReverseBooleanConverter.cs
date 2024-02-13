using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Converters
{
    public class ReverseBooleanConverter : IValueConverter
    {
        #region METHODS

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolean) 
            {
                return !boolean;
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => Convert(value, targetType, parameter, language);

        #endregion
    }
}
