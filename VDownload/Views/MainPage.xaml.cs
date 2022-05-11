using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload.Views
{
    public sealed partial class MainPage : Page
    {
        #region CONSTANTS

        private readonly Dictionary<string, Type> Pages = new Dictionary<string, Type>()
        {
            {"home", typeof(Home.MainPage)},
            {"subscriptions", typeof(Subscriptions.MainPage)},
            {"about", typeof(About.MainPage)},
            {"sources", typeof(Sources.MainPage)},
            {"settings", typeof(Settings.MainPage)},
        };

        #endregion



        #region CONSTRUCTORS

        public MainPage()
        {
            this.InitializeComponent();

            // Hide default title bar.
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);

            // Navigate to home page
            ContentFrame.Navigate(Pages["home"]);
            NavigationPanel.SelectedItem = NavigationPanel.MenuItems[0];
        }

        #endregion



        #region EVENT HANDLERS

        private void NavigationPanel_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(Pages["settings"], args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null)
            {
                ContentFrame.Navigate(Pages[args.InvokedItemContainer.Tag.ToString()], args.RecommendedNavigationTransitionInfo);
            }
        }

        #endregion
    }
}
