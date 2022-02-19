using System;
using VDownload.Core.EventArgsObjects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload.Views.Home
{
    public sealed partial class HomeMain : Page
    {
        #region CONSTRUCTORS

        public HomeMain()
        {
            this.InitializeComponent();
        }

        #endregion



        #region BUTTONS EVENTS

        // ADD VIDEO BUTTON CHECKED
        private void HomeOptionsBarAddVideoButton_Checked(object sender, RoutedEventArgs e)
        {
            // Uncheck playlist button
            HomeOptionsBarAddPlaylistButton.IsChecked = false;

            // Create video adding control
            HomeOptionsBarAddVideoControl homeOptionsBarAddVideoControl = new HomeOptionsBarAddVideoControl();
            homeOptionsBarAddVideoControl.SearchButtonClicked += HomeOptionsBarAddVideoControl_SearchButtonClicked;
            HomeOptionsBarAddingControl.Content = homeOptionsBarAddVideoControl;
        }

        // ADD VIDEO SEARCH BUTTON CLICKED
        private void HomeOptionsBarAddVideoControl_SearchButtonClicked(object sender, VideoSearchEventArgs e)
        {
            
        }


        // ADD PLAYLIST BUTTON CHECKED
        private void HomeOptionsBarAddPlaylistButton_Checked(object sender, RoutedEventArgs e)
        {
            // Uncheck video button
            HomeOptionsBarAddVideoButton.IsChecked = false;

            // Create playlist adding control
            HomeOptionsBarAddPlaylistControl homeOptionsBarAddPlaylistControl = new HomeOptionsBarAddPlaylistControl();
            homeOptionsBarAddPlaylistControl.SearchButtonClicked += HomeOptionsBarAddPlaylistControl_SearchButtonClicked;
            HomeOptionsBarAddingControl.Content = homeOptionsBarAddPlaylistControl;
        }

        // ADD PLAYLIST SEARCH BUTTON CLICKED
        private void HomeOptionsBarAddPlaylistControl_SearchButtonClicked(object sender, PlaylistSearchEventArgs e)
        {
            
        }


        // ADDING BUTTONS UNCHECKED
        private void HomeOptionsBarAddingButtons_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeOptionsBarAddingControl.Content = null;
        }

        #endregion
    }
}
