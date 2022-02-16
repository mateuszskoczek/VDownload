// Internal

// System
using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using System.Collections.Generic;

namespace VDownload.GUI.Views
{
    public sealed partial class MainPage : Page
    {
        #region INIT

        // CONSTRUCTOR
        public MainPage()
        {
            InitializeComponent();

            // Hide default title bar.
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(AppTitleBar);

            // Navigate to home page
            MainPageContentFrame.Navigate(Pages["home"]);
            MainPageNavigationPanel.SelectedItem = MainPageNavigationPanel.MenuItems[0];
        }

        #endregion



        #region NAVIGATION PANEL

        // PAGES DICTIONARY
        private Dictionary<string, Type> Pages = new Dictionary<string, Type>()
        {
            {"home", typeof(Home.HomeMain)},
            {"subscriptions", typeof(Subscriptions.SubscriptionsMain)},
            {"sources", typeof(Sources.SourcesMain)},
            {"settings", typeof(Settings.SettingsMain)}
        };

        // ON ITEM INVOKED
        private void MainPageNavigationPanel_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked) MainPageContentFrame.Navigate(Pages["settings"], args.RecommendedNavigationTransitionInfo);
            else if (args.InvokedItemContainer != null) MainPageContentFrame.Navigate(Pages[args.InvokedItemContainer.Tag.ToString()], args.RecommendedNavigationTransitionInfo);
        }

        #endregion

    }
}
