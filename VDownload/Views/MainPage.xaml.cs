using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload.Views
{
    public sealed partial class MainPage : Page
    {
        #region CONSTRUCTORS

        public MainPage()
        {
            this.InitializeComponent();

            // Hide default title bar.
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
        private readonly Dictionary<string, Type> Pages = new Dictionary<string, Type>()
        {
            {"home", typeof(Home.HomeMain)},
            {"subscriptions", typeof(Subscriptions.MainPage)},
            {"about", typeof(About.AboutMain)},
            {"sources", typeof(Sources.MainPage)},
            {"settings", typeof(Settings.SettingsMain)},
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
