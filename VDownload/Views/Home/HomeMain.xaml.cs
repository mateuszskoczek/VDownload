using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.ApplicationModel.Resources;
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

        // HOME TASKS LIST PLACEHOLDER
        private readonly HomeTasksListPlaceholder HomeTasksListPlaceholder = new HomeTasksListPlaceholder();

        // HOME VIDEOS LIST
        private static ContentControl HomeTasksListPlaceCurrent = null;
        private static StackPanel HomeTasksList = null;
        public static List<HomeTaskPanel> TaskPanelsList = new List<HomeTaskPanel>();

        #endregion



        #region EVENT HANDLERS VOIDS

        // ON NAVIGATED TO
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            HomeTasksListPlaceCurrent = HomeTasksListPlace;
            if (HomeTasksList != null) HomeTasksList.Children.Clear();
            HomeTasksList = new StackPanel { Spacing = 10 };
            if (TaskPanelsList.Count > 0)
            {
                foreach (HomeTaskPanel homeVideoPanel in TaskPanelsList) HomeTasksList.Children.Add(homeVideoPanel);
                HomeTasksListPlace.Content = HomeTasksList;
            }
            else
            {
                HomeTasksListPlace.Content = HomeTasksListPlaceholder;
            }
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
            HomeOptionBarAndAddingPanelRow.Height = GridLength.Auto;
            HomeTasksListRow.Height = new GridLength(1, GridUnitType.Star);

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
                HomeOptionsBarSearchingStatusControl.Content = null;


                HomeOptionBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                HomeTasksListRow.Height = new GridLength(0);
                HomeVideoAddingPanel addingPanel = new HomeVideoAddingPanel(videoService);
                addingPanel.VideoAddRequest += HomeVideoAddingPanel_VideoAddRequest;
                HomeAddingPanel.Content = addingPanel;
            }
        }

        private void HomeVideoAddingPanel_VideoAddRequest(object sender, VideoAddEventArgs e)
        {
            // Replace placeholder
            HomeTasksListPlace.Content = HomeTasksList;

            // Uncheck video button
            HomeOptionsBarAddVideoButton.IsChecked = false;

            // Create video task
            HomeTaskPanel taskPanel = new HomeTaskPanel(e.VideoService, e.MediaType, e.Stream, e.TrimStart, e.TrimEnd, e.Filename, e.Extension, e.Location, e.Schedule);

            taskPanel.TaskRemovingRequested += (s, a) =>
            {
                // Remove task from tasks lists
                TaskPanelsList.Remove(taskPanel);
                HomeTasksList.Children.Remove(taskPanel);
                if (TaskPanelsList.Count <= 0) HomeTasksListPlace.Content = HomeTasksListPlaceholder;
            };

            // Add task to tasks lists
            HomeTasksList.Children.Add(taskPanel);
            TaskPanelsList.Add(taskPanel);
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
            HomeTasksListRow.Height = new GridLength(1, GridUnitType.Star);

            HomeAddingPanel.Content = null;
            HomeOptionsBarAddingControl.Content = null;
            HomeOptionsBarSearchingStatusControl.Content = null;
        }


        // DOWNLOAD ALL BUTTON CLICKED
        private async void HomeOptionsBarDownloadAllButton_Click(object sender, RoutedEventArgs e)
        {
            HomeTaskPanel[] idleTasks = TaskPanelsList.Where((HomeTaskPanel video) => video.TaskStatus == Core.Enums.TaskStatus.Idle).ToArray();
            if (idleTasks.Count() > 0)
            {
                bool delay = (bool)Config.GetValue("delay_task_when_queued_task_starts_on_metered_network");
                ContentDialogResult dialogResult = await new ContentDialog
                {
                    Title = ResourceLoader.GetForCurrentView().GetString("HomeDownloadAllButtonMeteredConnectionDialogTitle"),
                    Content = ResourceLoader.GetForCurrentView().GetString("HomeDownloadAllButtonMeteredConnectionDialogDescription"),
                    PrimaryButtonText = ResourceLoader.GetForCurrentView().GetString("HomeDownloadAllButtonMeteredConnectionDialogStartAndDelayText"),
                    SecondaryButtonText = ResourceLoader.GetForCurrentView().GetString("HomeDownloadAllButtonMeteredConnectionDialogStartWithoutDelayText"),
                    CloseButtonText = ResourceLoader.GetForCurrentView().GetString("HomeDownloadAllButtonMeteredConnectionDialogCancel"),
                }.ShowAsync();
                switch (dialogResult)
                {
                    case ContentDialogResult.Primary: delay = true; break;
                    case ContentDialogResult.Secondary: delay = false; break;
                    case ContentDialogResult.None: return;
                }

                foreach (HomeTaskPanel videoPanel in idleTasks)
                {
                    await Task.Delay(50);
                    videoPanel.Start(delay);
                }
            }
        }

        #endregion



        #region METHODS

        // WAIT IN QUEUE
        public static async Task WaitInQueue(bool delayWhenOnMeteredConnection, CancellationToken token)
        {
            while ((TaskPanelsList.Where((HomeTaskPanel video) => video.TaskStatus == Core.Enums.TaskStatus.InProgress).Count() >= (int)Config.GetValue("max_active_video_task") || (delayWhenOnMeteredConnection && NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)) && !token.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        #endregion
    }
}
