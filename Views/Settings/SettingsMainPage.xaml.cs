using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Core;
using Windows.System;
using System.Diagnostics;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.Views.Settings
{
    public sealed partial class SettingsMainPage : ContentDialog
    {

        private readonly List<(string Tag, Type Page)> pages = new List<(string Tag, Type Page)>
        {
            ("general", typeof(SettingsGeneralPage)),
            ("options", typeof(SettingsOptionsPage)),
            ("about", typeof(SettingsAboutPage)),
        };

        public SettingsMainPage()
        {
            this.InitializeComponent();
        }

        private void Menu_Loaded(object sender, RoutedEventArgs e)
        {
            MenuFrame.Navigated += On_Navigated;
            Menu.SelectedItem = Menu.MenuItems[0];
            Menu_Navigate("general", new EntranceNavigationTransitionInfo());
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;
            Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;
        }

        private void Menu_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer != null)
            {
                var menuItemTag = args.InvokedItemContainer.Tag.ToString();
                Menu_Navigate(menuItemTag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void Menu_Navigate(string menuItemTag, NavigationTransitionInfo transitionInfo)
        {
            Type page = null;
            var item = pages.FirstOrDefault(p => p.Tag.Equals(menuItemTag));
            page = item.Page;

            var preMenuPageType = MenuFrame.CurrentSourcePageType;

            if (!(page is null) && !Equals(preMenuPageType, page))
            {
                _ = MenuFrame.Navigate(page, null, transitionInfo);
            }
        }

        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            // When Alt+Left are pressed navigate back
            if (e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
                && e.VirtualKey == VirtualKey.Left
                && e.KeyStatus.IsMenuKeyDown == true
                && !e.Handled)
            {
                e.Handled = TryGoBack();
            }
        }

        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
        {
            // Handle mouse back button.
            if (e.CurrentPoint.Properties.IsXButton1Pressed)
            {
                e.Handled = TryGoBack();
            }
        }

        private bool TryGoBack()
        {
            if (!MenuFrame.CanGoBack)
                return false;

            // Don't go back if the nav pane is overlayed.
            if (Menu.IsPaneOpen &&
                (Menu.DisplayMode == NavigationViewDisplayMode.Compact ||
                 Menu.DisplayMode == NavigationViewDisplayMode.Minimal))
                return false;

            MenuFrame.GoBack();
            return true;
        }

        private void MenuFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            if (MenuFrame.SourcePageType != null)
            {
                var item = pages.FirstOrDefault(p => p.Page == e.SourcePageType);
                Menu.SelectedItem = Menu.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void Menu_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {

        }
    }
}
