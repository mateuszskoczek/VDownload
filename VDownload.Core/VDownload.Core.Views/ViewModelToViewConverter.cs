using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using VDownload.Core.ViewModels.About;
using VDownload.Core.ViewModels.Authentication;
using VDownload.Core.ViewModels.Home;
using VDownload.Core.ViewModels.Settings;
using VDownload.Core.ViewModels.Subscriptions;
using VDownload.Core.Views.About;
using VDownload.Core.Views.Authentication;
using VDownload.Core.Views.Home;
using VDownload.Core.Views.Settings;
using VDownload.Core.Views.Subscriptions;

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
            { typeof(HomeVideoCollectionViewModel), typeof(HomeVideoCollectionView) },
            { typeof(SubscriptionsViewModel), typeof(SubscriptionsView) },
            { typeof(SettingsViewModel), typeof(SettingsView) },
            { typeof(AboutViewModel), typeof(AboutView) },
            { typeof(AuthenticationViewModel), typeof(AuthenticationView) }
        };

        protected static IServiceProvider _serviceProvider;

        #endregion



        #region PUBLIC METHODS

        public static void Initialize(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return null;
            }
            if (value is Type type && _viewModelViewBinding.ContainsKey(type))
            {
                return _serviceProvider.GetService(_viewModelViewBinding[type]);
            }
            if (_viewModelViewBinding.ContainsKey(value.GetType()))
            {
                return _serviceProvider.GetService(_viewModelViewBinding[value.GetType()]);
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
