using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home
{
    public sealed partial class HomeOptionsBarPlaylistSearch : UserControl
    {
        #region CONSTANTS

        // RESOURCES
        private static readonly ResourceDictionary IconRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };

        // SEARCHING STATUS CONTROLS
        private static readonly Microsoft.UI.Xaml.Controls.ProgressRing HomeOptionsBarPlaylistSearchStatusProgressRing = new Microsoft.UI.Xaml.Controls.ProgressRing { Width = 15, Height = 15, Margin = new Thickness(15, 5, 5, 5), IsActive = true };
        private static readonly Image HomeOptionsBarPlaylistSearchStatusErrorImage = new Image { Width = 15, Height = 15, Margin = new Thickness(15, 5, 5, 5), Source = (SvgImageSource)IconRes["ErrorIcon"] };

        #endregion



        #region CONSTRUCTORS

        public HomeOptionsBarPlaylistSearch()
        {
            this.InitializeComponent();

            // Set default max videos
            HomeOptionsBarPlaylistSearchControlMaxVideosNumberBox.Value = (int)Config.GetValue("default_max_playlist_videos");
        }

        #endregion



        #region PROPERTIES

        public CancellationToken CancellationToken { get; set; }

        #endregion



        #region EVENT HANDLERS

        // NUMBERBOX FOCUS LOST
        private void HomeOptionsBarPlaylistSearchControlMaxVideosNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(HomeOptionsBarPlaylistSearchControlMaxVideosNumberBox.Value)) HomeOptionsBarPlaylistSearchControlMaxVideosNumberBox.Value = (int)Config.GetValue("default_max_playlist_videos");
        }

        // SEARCH BUTTON CLICKED
        private async void HomeOptionsBarPlaylistSearchControlSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Invoke search button click event
            SearchButtonClick?.Invoke(this, EventArgs.Empty);

            // Close info box
            HomeOptionsBarPlaylistSearchControlInfoBox.IsOpen = false;

            // Set SearchingStatusControl
            HomeOptionsBarPlaylistSearchControlStatusControl.Content = HomeOptionsBarPlaylistSearchStatusProgressRing;

            // Parse url
            (PlaylistSource Type, string ID) source = Source.GetPlaylistSource(HomeOptionsBarPlaylistSearchControlUrlTextBox.Text);

            // Check url
            if (source.Type == PlaylistSource.Null)
            {
                HomeOptionsBarPlaylistSearchControlStatusControl.Content = HomeOptionsBarPlaylistSearchStatusErrorImage;
            }
            else
            {
                // Select video service
                IPlaylistService playlistService = null;
                switch (source.Type)
                {
                    case PlaylistSource.TwitchChannel: playlistService = new Core.Services.Sources.Twitch.Channel(source.ID); break;
                }

                // Get metadata and streams
                try
                {
                    await playlistService.GetMetadataAsync(CancellationToken);
                    await playlistService.GetVideosAsync((int)Math.Round(HomeOptionsBarPlaylistSearchControlMaxVideosNumberBox.Value), CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    HomeOptionsBarPlaylistSearchControlStatusControl.Content = null;
                    return;
                }
                catch (MediaNotFoundException)
                {
                    HomeOptionsBarPlaylistSearchControlStatusControl.Content = HomeOptionsBarPlaylistSearchStatusErrorImage;
                    return;
                }
                catch (TwitchAccessTokenNotFoundException)
                {
                    HomeOptionsBarPlaylistSearchControlStatusControl.Content = HomeOptionsBarPlaylistSearchStatusErrorImage;
                    ContentDialog twitchAccessTokenNotFoundErrorDialog = new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingErrorDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingTwitchAccessTokenNotFoundErrorDialogDescription"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                    };
                    await twitchAccessTokenNotFoundErrorDialog.ShowAsync();
                    return;
                }
                catch (TwitchAccessTokenNotValidException)
                {
                    HomeOptionsBarPlaylistSearchControlStatusControl.Content = HomeOptionsBarPlaylistSearchStatusErrorImage;
                    ContentDialog twitchAccessTokenNotValidErrorDialog = new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingErrorDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingTwitchAccessTokenNotValidErrorDialogDescription"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                    };
                    await twitchAccessTokenNotValidErrorDialog.ShowAsync();
                    return;
                }
                catch (WebException ex)
                {
                    HomeOptionsBarPlaylistSearchControlStatusControl.Content = HomeOptionsBarPlaylistSearchStatusErrorImage;
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        ContentDialog internetAccessErrorDialog = new ContentDialog
                        {
                            Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingErrorDialogTitle"),
                            Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingInternetConnectionErrorDialogDescription"),
                            CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                        };
                        await internetAccessErrorDialog.ShowAsync();
                        return;
                    }
                    else throw;
                }

                // Set searching status control to done (null)
                HomeOptionsBarPlaylistSearchControlStatusControl.Content = null;
                
                // Invoke search successed event
                PlaylistSearchSuccessed?.Invoke(this, new PlaylistSearchSuccessedEventArgs { PlaylistService = playlistService });
            }
        }

        // HELP BUTTON CLICKED
        private void HomeOptionsBarPlaylistSearchControlHelpButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch info box
            HomeOptionsBarPlaylistSearchControlInfoBox.IsOpen = !HomeOptionsBarPlaylistSearchControlInfoBox.IsOpen;
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<PlaylistSearchSuccessedEventArgs> PlaylistSearchSuccessed;
        public event EventHandler SearchButtonClick;

        #endregion
    }
}
