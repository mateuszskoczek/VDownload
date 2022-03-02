﻿using System;
using VDownload.Core.EventArgsObjects;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload.Views.Home
{
    public sealed partial class HomeOptionsBarAddVideoControl : UserControl
    {
        #region CONSTRUCTORS

        public HomeOptionsBarAddVideoControl()
        {
            this.InitializeComponent();
        }

        #endregion



        #region PROPERTIES

        // SEARCH BUTTON EVENT HANDLER
        public event EventHandler<VideoSearchEventArgs> SearchButtonClicked;

        #endregion



        #region EVENT HANDLERS

        // SEARCH BUTTON CLICKED
        private void HomeOptionsBarAddVideoControlSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Close info box
            HomeOptionsBarAddVideoControlInfoBox.IsOpen = false;

            // Invoke search button event handlers
            VideoSearchEventArgs args = new VideoSearchEventArgs
            {
                Phrase = HomeOptionsBarAddVideoControlUrlTextBox.Text
            };
            SearchButtonClicked?.Invoke(this, args);
        }

        // HELP BUTTON CLICKED
        private void HomeOptionsBarAddVideoControlHelpButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch info box
            HomeOptionsBarAddVideoControlInfoBox.IsOpen = !HomeOptionsBarAddVideoControlInfoBox.IsOpen;
        }

        #endregion
    }
}