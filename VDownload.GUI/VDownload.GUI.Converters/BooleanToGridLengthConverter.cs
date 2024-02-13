using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Converters
{
    public class BooleanToGridLengthConverter : IValueConverter
    {
        #region METHODS

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            GridLength notVisibleLength = new GridLength(0);
            if (value is bool visible)
            {
                GridLength visibleLength = new GridLength(1, GridUnitType.Star);
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
                return visible ? visibleLength : notVisibleLength;
            }
            return notVisibleLength;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
