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
    public sealed partial class HomeOptionsBarVideoSearch : UserControl
    {
        #region CONSTANTS

        // RESOURCES
        private static readonly ResourceDictionary IconRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };

        // SEARCHING STATUS CONTROLS
        private static readonly Microsoft.UI.Xaml.Controls.ProgressRing HomeOptionsBarVideoSearchStatusProgressRing = new Microsoft.UI.Xaml.Controls.ProgressRing { Width = 15, Height = 15, Margin = new Thickness(15, 5, 5, 5), IsActive = true };
        private static readonly Image HomeOptionsBarVideoSearchStatusErrorImage = new Image { Width = 15, Height = 15, Margin = new Thickness(15, 5, 5, 5), Source = (SvgImageSource)IconRes["ErrorIcon"] };

        #endregion



        #region CONSTRUCTORS

        public HomeOptionsBarVideoSearch()
        {
            this.InitializeComponent();
        }

        #endregion



        #region PROPERTIES

        public CancellationToken CancellationToken { get; set; }

        #endregion



        #region EVENT HANDLERS VOIDS

        // SEARCH BUTTON CLICKED
        private async void HomeOptionsBarVideoSearchControlSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Invoke search button click event
            SearchButtonClick?.Invoke(this, EventArgs.Empty);

            // Close info box
            HomeOptionsBarVideoSearchControlInfoBox.IsOpen = false;

            // Set SearchingStatusControl
            HomeOptionBarVideoSearchControlStatusControl.Content = HomeOptionsBarVideoSearchStatusProgressRing;

            // Parse url
            (VideoSource Type, string ID) source = Source.GetVideoSource(HomeOptionsBarVideoSearchControlUrlTextBox.Text);

            // Check url
            if (source.Type == VideoSource.Null)
            {
                HomeOptionBarVideoSearchControlStatusControl.Content = HomeOptionsBarVideoSearchStatusErrorImage;
            }
            else
            {
                // Select video service
                IVideoService videoService = null;
                switch (source.Type)
                {
                    case VideoSource.TwitchVod: videoService = new Core.Services.Sources.Twitch.Vod(source.ID); break;
                    case VideoSource.TwitchClip: videoService = new Core.Services.Sources.Twitch.Clip(source.ID); break;
                }

                // Get metadata and streams
                try
                {
                    await videoService.GetMetadataAsync(CancellationToken);
                    await videoService.GetStreamsAsync(CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    HomeOptionBarVideoSearchControlStatusControl.Content = null;
                    return;
                }
                catch (MediaNotFoundException)
                {
                    HomeOptionBarVideoSearchControlStatusControl.Content = HomeOptionsBarVideoSearchStatusErrorImage;
                    return;
                }
                catch (TwitchAccessTokenNotFoundException)
                {
                    HomeOptionBarVideoSearchControlStatusControl.Content = HomeOptionsBarVideoSearchStatusErrorImage;
                    ContentDialog twitchAccessTokenNotFoundErrorDialog = new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarVideoSearchingErrorDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarVideoSearchingTwitchAccessTokenNotFoundErrorDialogDescription"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                    };
                    await twitchAccessTokenNotFoundErrorDialog.ShowAsync();
                    return;
                }
                catch (TwitchAccessTokenNotValidException)
                {
                    HomeOptionBarVideoSearchControlStatusControl.Content = HomeOptionsBarVideoSearchStatusErrorImage;
                    ContentDialog twitchAccessTokenNotValidErrorDialog = new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarVideoSearchingErrorDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarVideoSearchingTwitchAccessTokenNotValidErrorDialogDescription"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                    };
                    await twitchAccessTokenNotValidErrorDialog.ShowAsync();
                    return;
                }
                catch (WebException wex)
                {
                    HomeOptionBarVideoSearchControlStatusControl.Content = HomeOptionsBarVideoSearchStatusErrorImage;
                    if (wex.Response is null)
                    {
                        ContentDialog internetAccessErrorDialog = new ContentDialog
                        {
                            Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarVideoSearchingErrorDialogTitle"),
                            Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarVideoSearchingInternetConnectionErrorDialogDescription"),
                            CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                        };
                        await internetAccessErrorDialog.ShowAsync();
                        return;
                    }
                    else throw;
                }

                // Set searching status control to done (null)
                HomeOptionBarVideoSearchControlStatusControl.Content = null;

                // Invoke search successed event
                VideoSearchSuccessed?.Invoke(this, new VideoSearchSuccessedEventArgs { VideoService = videoService });
            }
        }

        // HELP BUTTON CLICKED
        private void HomeOptionsBarVideoSearchControlHelpButton_Click(object sender, RoutedEventArgs e)
        {
            // Switch info box
            HomeOptionsBarVideoSearchControlInfoBox.IsOpen = !HomeOptionsBarVideoSearchControlInfoBox.IsOpen;
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<VideoSearchSuccessedEventArgs> VideoSearchSuccessed;
        public event EventHandler SearchButtonClick;

        #endregion
    }
}
