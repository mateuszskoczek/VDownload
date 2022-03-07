using System;
using VDownload.Core.EventArgs;
using VDownload.Core.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload.Views.Home
{
    public sealed partial class HomeOptionsBarAddPlaylistControl : UserControl
    {
        #region CONSTRUCTORS

        public HomeOptionsBarAddPlaylistControl()
        {
            this.InitializeComponent();
        }

        #endregion



        #region PROPERTIES

        // MAX VIDEOS NUMBERBOX DEFAULT VALUE
        public int DefaultMaxPlaylistVideos = (int)Config.GetValue("default_max_playlist_videos");

        #endregion



        #region EVENT HANDLERS

        // NUMBERBOX FOCUS LOST
        private void HomeOptionsBarAddPlaylistControlMaxVideosNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(HomeOptionsBarAddPlaylistControlMaxVideosNumberBox.Value)) HomeOptionsBarAddPlaylistControlMaxVideosNumberBox.Value = DefaultMaxPlaylistVideos;
        }

        // SEARCH BUTTON CLICKED
        private void HomeOptionsBarAddPlaylistControlSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Close info box
            HomeOptionsBarAddPlaylistControlInfoBox.IsOpen = false;

            // Invoke search button event handlers
            PlaylistSearchEventArgs args = new PlaylistSearchEventArgs
            {
                Url = HomeOptionsBarAddPlaylistControlUrlTextBox.Text,
                VideosCount = int.Parse(HomeOptionsBarAddPlaylistControlMaxVideosNumberBox.Text),
            };
            SearchButtonClicked?.Invoke(this, args);
        }

        // HELP BUTTON CLICKED
        private void HomeOptionsBarAddPlaylistControlHelpButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch info box
            HomeOptionsBarAddPlaylistControlInfoBox.IsOpen = !HomeOptionsBarAddPlaylistControlInfoBox.IsOpen;
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<PlaylistSearchEventArgs> SearchButtonClicked;

        #endregion
    }
}
