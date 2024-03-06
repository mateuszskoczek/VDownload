using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using VDownload.Core.ViewModels.About;
using VDownload.Core.ViewModels.Authentication;
using VDownload.Core.ViewModels.Home;
using VDownload.Core.ViewModels.Settings;
using VDownload.Core.Views.About;
using VDownload.Core.Views.Authentication;
using VDownload.Core.Views.Home;
using VDownload.Core.Views.Settings;

namespace VDownload.Core.Views
{
    public class ViewModelToViewConverter : IValueConverter
    {
        #region FIELDS

        private readonly Dictionary<Type, Type> _viewModelViewBinding = new Dictionary<Type, Type>
        {
            { typeof(HomeViewModel), typeof(HomeView) },
            { typeof(HomeDownloadsViewModel), typeof(HomeDownloadsView) },
            { typeof(HomeVideoViewModel), typeof(HomeVideoView) },
            { typeof(HomePlaylistViewModel), typeof(HomePlaylistView) },
            { typeof(SettingsViewModel), typeof(SettingsView) },
            { typeof(AboutViewModel), typeof(AboutView) },
            { typeof(AuthenticationViewModel), typeof(AuthenticationView) }
        };

        #endregion



        #region PROPERTIES

        public static IServiceProvider ServiceProvider { protected get; set; }

        #endregion



        #region PUBLIC METHODS

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return null;
            }
            if (value is Type type && _viewModelViewBinding.ContainsKey(type))
            {
                return ServiceProvider.GetService(_viewModelViewBinding[type]);
            }
            if (_viewModelViewBinding.ContainsKey(value.GetType()))
            {
                return ServiceProvider.GetService(_viewModelViewBinding[value.GetType()]);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Dictionary<Type, Type> viewViewModelBinding = _viewModelViewBinding.ToDictionary(x => x.Value, x => x.Key);
            if (viewViewModelBinding.ContainsKey(value.GetType()))
            {
                return viewViewModelBinding[value.GetType()];
            }
            return null;
        }

        #endregion
    }
}
