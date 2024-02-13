using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.GUI.ViewModels;
using VDownload.GUI.Views;
using VDownload.Services;

namespace VDownload.GUI.Converters
{
    public class ViewModelToViewConverter : IValueConverter
    {
        #region FIELDS

        private readonly Dictionary<Type, Type> _viewModelViewBinding = new Dictionary<Type, Type>
        {
            { typeof(HomeViewModel), typeof(HomeView) },
            { typeof(SettingsViewModel), typeof(SettingsView) },
            { typeof(AuthenticationViewModel), typeof(AuthenticationView) }
        };
        private readonly Dictionary<Type, Type> _viewViewModelBinding = new Dictionary<Type, Type>
        {
            { typeof(HomeView), typeof(HomeViewModel) },
            { typeof(SettingsView), typeof(SettingsViewModel) },
            { typeof(AuthenticationView), typeof(AuthenticationViewModel) }
        };

        #endregion



        #region METHODS

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return null;
            }
            if (value is Type type && _viewModelViewBinding.ContainsKey(type))
            {
                return ServiceProvider.Instance.GetService(_viewModelViewBinding[type]);
            }
            if (_viewModelViewBinding.ContainsKey(value.GetType()))
            {
                return ServiceProvider.Instance.GetService(_viewModelViewBinding[value.GetType()]);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (_viewViewModelBinding.ContainsKey(value.GetType()))
            {
                return _viewViewModelBinding[value.GetType()];
            }
            return null;
        }

        #endregion
    }
}
