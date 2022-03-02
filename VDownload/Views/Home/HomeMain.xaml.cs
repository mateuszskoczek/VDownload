using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgsObjects;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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



        #region PROPERTIES

        // SEARCHING STATUS CONTROLS
        private readonly Microsoft.UI.Xaml.Controls.ProgressRing HomeOptionsBarSearchingStatusProgressRing = new Microsoft.UI.Xaml.Controls.ProgressRing { Width = 15, Height = 15, Margin = new Thickness(5), IsActive = true };
        private readonly Image HomeOptionsBarSearchingStatusErrorImage = new Image { Width = 15, Height = 15, Margin = new Thickness(5), Source = (SvgImageSource)new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") }["ErrorIcon"] };

        // CANCELLATON TOKEN
        private CancellationTokenSource SearchingCancellationToken = new CancellationTokenSource();

        // HOME VIDEOS LIST
        private static StackPanel HomeVideosListOldPanel = null;
        public static List<HomeVideoPanel> VideoPanelsList = new List<HomeVideoPanel>();

        #endregion



        #region EVENT HANDLERS VOIDS

        // ON NAVIGATED TO
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (HomeVideosListOldPanel != null) HomeVideosListOldPanel.Children.Clear();
            HomeVideosListOldPanel = HomeVideosList;
            foreach (HomeVideoPanel homeVideoPanel in VideoPanelsList) HomeVideosList.Children.Add(homeVideoPanel);
        }

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
        private async void HomeOptionsBarAddVideoControl_SearchButtonClicked(object sender, VideoSearchEventArgs e)
        {
            HomeAddingPanel.Content = null;

            // Cancel previous operations
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();

            // Set SearchingStatusControl
            HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusProgressRing;

            // Parse url
            (VideoSource Type, string ID) source = Source.GetVideoSource(e.Phrase);

            // Check url
            if (source.Type == VideoSource.Null)
            {
                HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
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
                    await videoService.GetMetadataAsync(SearchingCancellationToken.Token);
                    await videoService.GetStreamsAsync(SearchingCancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    HomeOptionsBarSearchingStatusControl.Content = null;
                    return;
                }
                catch (TwitchAccessTokenNotFoundException)
                {
                    HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
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
                    HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
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
                    HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
                    if (wex.Response == null)
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
                HomeOptionsBarSearchingStatusControl.Content = null;


                HomeOptionBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                HomeVideosListRow.Height = new GridLength(0);
                HomeVideoAddingPanel addingPanel = new HomeVideoAddingPanel(videoService);
                addingPanel.VideoAddRequest += HomeVideoAddingPanel_VideoAddRequest;
                HomeAddingPanel.Content = addingPanel;
            }
        }

        private void HomeVideoAddingPanel_VideoAddRequest(object sender, VideoAddEventArgs e)
        {
            // Uncheck video button
            HomeOptionsBarAddVideoButton.IsChecked = false;

            // Create video panel/task
            HomeVideoPanel videoPanel = new HomeVideoPanel(e.VideoService, e.MediaType, e.Stream, e.TrimStart, e.TrimEnd, e.Filename, e.Extension, e.Location);

            videoPanel.VideoRemovingRequested += (s, a) =>
            {
                // Remove video panel/task from videos list
                VideoPanelsList.Remove(videoPanel);
                HomeVideosList.Children.Remove(videoPanel);
            };

            // Add video panel/task to videos list
            HomeVideosList.Children.Add(videoPanel);
            VideoPanelsList.Add(videoPanel);
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
        private async void HomeOptionsBarAddPlaylistControl_SearchButtonClicked(object sender, PlaylistSearchEventArgs e)
        {
            // Cancel previous operations
            SearchingCancellationToken.Cancel();

            // Set SearchingStatusControl
            HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusProgressRing;

            // Parse url
            (PlaylistSource Type, string ID) source = Source.GetPlaylistSource(e.Phrase);

            // Check url
            if (source.Type == PlaylistSource.Null)
            {
                HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
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
                    await playlistService.GetMetadataAsync(SearchingCancellationToken.Token);
                    await playlistService.GetVideosAsync(e.Count, SearchingCancellationToken.Token);
                }
                catch (OperationCanceledException)
                {
                    HomeOptionsBarSearchingStatusControl.Content = null;
                    return;
                }
                catch (TwitchAccessTokenNotFoundException)
                {
                    HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
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
                    HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
                    ContentDialog twitchAccessTokenNotValidErrorDialog = new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingErrorDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("HomeOptionsBarPlaylistSearchingTwitchAccessTokenNotValidErrorDialogDescription"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                    };
                    await twitchAccessTokenNotValidErrorDialog.ShowAsync();
                    return;
                }
                catch (WebException wex)
                {
                    HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusErrorImage;
                    if (wex.Response == null)
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
            }
        }


        // ADDING BUTTONS UNCHECKED
        private void HomeOptionsBarAddingButtons_Unchecked(object sender, RoutedEventArgs e)
        {
            // Cancel searching operations
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();

            HomeOptionBarAndAddingPanelRow.Height = GridLength.Auto;
            HomeVideosListRow.Height = new GridLength(1, GridUnitType.Star);

            HomeAddingPanel.Content = null;
            HomeOptionsBarAddingControl.Content = null;
            HomeOptionsBarSearchingStatusControl.Content = null;
        }


        // DOWNLOAD ALL BUTTON CLICKED
        private async void HomeOptionsBarDownloadAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HomeVideoPanel videoPanel in HomeVideosList.Children.Where(video => ((HomeVideoPanel)video).VideoStatus == VideoStatus.Idle))
            {
                await Task.Delay(50);
                videoPanel.Start();
            }
        }

        #endregion



        #region METHODS

        // WAIT IN QUEUE
        public static async Task WaitInQueue(CancellationToken token)
        {
            while (VideoPanelsList.Where(video => video.VideoStatus == VideoStatus.InProgress).Count() >= (int)Config.GetValue("max_active_video_task") && !token.IsCancellationRequested)
            {
                await Task.Delay(50);
            }
        }

        #endregion
    }
}
