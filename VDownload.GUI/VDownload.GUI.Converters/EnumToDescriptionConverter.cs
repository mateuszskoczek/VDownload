using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum enumValue)
            {
                return enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault()?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
