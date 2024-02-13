using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Converters
{
    public class BooleanToGridLengthFillReversedConverter : IValueConverter
    {
        #region METHODS

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            GridLength falseLength = GridLength.Auto;
            if (value is bool boolean)
            {
                GridLength trueLength = new GridLength(1, GridUnitType.Star);
                return boolean ? falseLength : trueLength;
            }
            return falseLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
