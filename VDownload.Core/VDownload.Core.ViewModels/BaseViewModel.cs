using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.ViewModels.Authentication;
using VDownload.Core.ViewModels.Home;
using VDownload.Core.ViewModels.Settings;
using VDownload.Services.UI.DictionaryResources;
using VDownload.Services.UI.StringResources;
using SimpleToolkit.UI.Models;
using VDownload.Core.ViewModels.About;

namespace VDownload.Core.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly IDictionaryResourcesService _dictionaryResourcesService;

        #endregion



        #region FIELDS

        protected readonly Type _settingsViewModel = typeof(SettingsViewModel);

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private Type _currentViewModel;

        [ObservableProperty]
        private NavigationViewItem _selectedItem;

        public ReadOnlyObservableCollection<NavigationViewItem> Items { get; protected set; }
        public ReadOnlyObservableCollection<NavigationViewItem> FooterItems { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public BaseViewModel(IStringResourcesService stringResourcesService, IDictionaryResourcesService dictionaryResourcesService)
        {
            _stringResourcesService = stringResourcesService;
            _dictionaryResourcesService = dictionaryResourcesService;

            Items = new ReadOnlyObservableCollection<NavigationViewItem>
            (
                new ObservableCollection<NavigationViewItem>
                {
                    new NavigationViewItem()
                    {
                        Name = _stringResourcesService.BaseViewResources.Get("HomeNavigationViewItem"),
                        IconSource = _dictionaryResourcesService.Get<string>("ImageBaseViewHome"),
                        ViewModel = typeof(HomeViewModel),
                    }
                }
            );
            FooterItems = new ReadOnlyObservableCollection<NavigationViewItem>
            (
                new ObservableCollection<NavigationViewItem>
                {
                    new NavigationViewItem()
                    {
                        Name = _stringResourcesService.BaseViewResources.Get("AboutNavigationViewItem"),
                        IconSource = _dictionaryResourcesService.Get<string>("ImageBaseViewAbout"),
                        ViewModel = typeof(AboutViewModel),
                    },
                    new NavigationViewItem()
                    {
                        Name = _stringResourcesService.BaseViewResources.Get("AuthenticationNavigationViewItem"),
                        IconSource = _dictionaryResourcesService.Get<string>("ImageBaseViewAuthentication"),
                        ViewModel = typeof(AuthenticationViewModel),
                    }
                }
            );

            SelectedItem = Items.First();
            CurrentViewModel = SelectedItem.ViewModel;
        }

        #endregion



        #region PUBLIC METHODS

        [RelayCommand]
        public void Navigate(Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs e)
        {
            if (e.IsSettingsInvoked)
            {
                CurrentViewModel = _settingsViewModel;
            }
            else
            {
                NavigationViewItem item = e.InvokedItemContainer.DataContext as NavigationViewItem;
                CurrentViewModel = item.ViewModel;
            }
        }

        #endregion
    }
}
