using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Converters
{
    public class BooleanToGridLengthReversedConverter : IValueConverter
    {
        #region METHODS

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            GridLength visibleLength = new GridLength(1, GridUnitType.Star);
            if (value is bool visible)
            {
                GridLength notVisibleLength = new GridLength(0);
                if (parameter is string width)
                {
                    if (width.ToLower() == "auto")
                    {
                        visibleLength = GridLength.Auto;
                    }
                    else if (int.TryParse(width, out int result))
                    {
                        visibleLength = new GridLength(result);
                    }
                }
                return visible ? notVisibleLength : visibleLength;
            }
            return visibleLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
