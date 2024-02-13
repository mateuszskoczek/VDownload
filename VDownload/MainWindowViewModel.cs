using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VDownload.GUI.Customs.Models;
using VDownload.GUI.Services.ResourceDictionaries;
using VDownload.GUI.ViewModels;
using VDownload.GUI.Views;

namespace VDownload
{
    public partial class MainWindowViewModel : ObservableObject
    {
        #region SERVICES

        private IResourceDictionariesServices _resourceDictionariesServices;

        #endregion



        #region FIELDS

        private readonly Type _settingsViewModel = typeof(SettingsViewModel);

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private Type _currentViewModel;

        [ObservableProperty]
        private NavigationViewItem _selectedItem;

        public ObservableCollection<NavigationViewItem> Items { get; set; }
        public ObservableCollection<NavigationViewItem> FooterItems { get; set; }

        #endregion 



        #region CONSTRUCTORS

        public MainWindowViewModel(IResourceDictionariesServices resourceDictionariesServices) 
        {
            _resourceDictionariesServices = resourceDictionariesServices;

            Items = new ObservableCollection<NavigationViewItem>
            {
                new NavigationViewItem
                {
                    Name = "Home",
                    IconSource = _resourceDictionariesServices.Images.NavigationViewHome,
                    ViewModel = typeof(HomeViewModel)
                }
            };
            FooterItems = new ObservableCollection<NavigationViewItem>
            {
                new NavigationViewItem
                {
                    Name = "Authentication",
                    IconSource = _resourceDictionariesServices.Images.NavigationViewAuthentication,
                    ViewModel = typeof(AuthenticationViewModel)
                }
            };

            SelectedItem = Items.FirstOrDefault();
            CurrentViewModel = SelectedItem.ViewModel;
        }

        #endregion



        #region COMMANDS

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
