// Internal
using VDownload.Views.AddVideo;
using VDownload.Sources;
using VDownload.Services;
using VDownload.Objects.Enums;

// System
using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload
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
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
        }

        #endregion



        #region CLICK BUTTON HANDLERS

        // DOWNLOAD ALL BUTTON
        private void AppBarDownloadAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Start all video tasks from listed videos list if they wasn't started
            foreach (VObject v in Videos.VideoObjectsList)
            {
                if (v.VideoStatus != VideoStatus.InProgress && v.VideoStatus != VideoStatus.Waiting)
                    v.Start();
            }
        }

        // ADD VIDEO BUTTON
        private async void AppBarAddVideoButton_Click(object sender, RoutedEventArgs e)
        {
            // Create window
            AddVideoBase addVideoPage = new AddVideoBase();
            ContentDialogResult result = await addVideoPage.ShowAsync();

            // Add video to list
            if (result == ContentDialogResult.Primary)
            {
                // Get video info
                VObject video = (VObject)addVideoPage.Content;

                // Create and attach video panel
                video.AddVideoToList(VideoPanel);
            }
        }

        // SETTINGS BUTTON
        private void AppBarSettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
