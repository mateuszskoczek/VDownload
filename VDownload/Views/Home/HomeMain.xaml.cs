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
using VDownload.Core.Structs;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Home
{
    public sealed partial class HomeMain : Page
    {
        #region CONSTANTS

        // RESOURCES
        private static readonly ResourceDictionary ImageRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };

        // SEARCHING STATUS CONTROLS
        private static readonly Microsoft.UI.Xaml.Controls.ProgressRing HomeOptionsBarSearchingStatusProgressRing = new Microsoft.UI.Xaml.Controls.ProgressRing { Width = 15, Height = 15, Margin = new Thickness(5), IsActive = true };
        private static readonly Image HomeOptionsBarSearchingStatusErrorImage = new Image { Width = 15, Height = 15, Margin = new Thickness(5), Source = (SvgImageSource)ImageRes["ErrorIcon"] };

        // TASKS LIST PLACEHOLDER
        private static readonly HomeTasksListPlaceholder HomeTasksListPlaceholder = new HomeTasksListPlaceholder();

        #endregion


        #region CONSTRUCTORS

        public HomeMain()
        {
            this.InitializeComponent();

            // Set cancellation token
            SearchingCancellationToken = new CancellationTokenSource();
        }

        #endregion



        #region PROPERTIES

        // CANCELLATON TOKEN
        private CancellationTokenSource SearchingCancellationToken { get; set; }

        // HOME TASKS LIST
        private static ContentControl HomeTasksListCurrentParent = null;
        private static StackPanel HomeTasksList = null;
        public static List<HomeTaskPanel> TasksList = new List<HomeTaskPanel>();

        #endregion



        #region EVENT HANDLERS VOIDS

        // ON NAVIGATED TO
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set current panel
            HomeTasksListCurrentParent = HomeTasksListParent;

            // Detach task panels from old task list
            if (HomeTasksList != null) HomeTasksList.Children.Clear();

            // Create new task list
            HomeTasksList = new StackPanel { Spacing = 10 };

            // Attach task panels to new task list
            if (TasksList.Count > 0)
            {
                foreach (HomeTaskPanel homeVideoPanel in TasksList) HomeTasksList.Children.Add(homeVideoPanel);
                HomeTasksListCurrentParent.Content = HomeTasksList;
            }
            else
            {
                HomeTasksListCurrentParent.Content = HomeTasksListPlaceholder;
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
            // Set UI
            HomeOptionBarAndAddingPanelRow.Height = GridLength.Auto;
            HomeTasksListRow.Height = new GridLength(1, GridUnitType.Star);
            HomeAddingPanel.Content = null;

            // Cancel previous operations
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();

            // Set SearchingStatusControl
            HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusProgressRing;

            // Parse url
            (VideoSource Type, string ID) source = Source.GetVideoSource(e.Url);

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

                // Set UI
                HomeOptionBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                HomeTasksListRow.Height = new GridLength(0);

                // Open adding panel
                HomeVideoAddingPanel addingPanel = new HomeVideoAddingPanel(videoService);
                addingPanel.TasksAddingRequested += HomeTasksAddingRequest;
                HomeAddingPanel.Content = addingPanel;
            }
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
            // Set UI
            HomeOptionBarAndAddingPanelRow.Height = GridLength.Auto;
            HomeTasksListRow.Height = new GridLength(1, GridUnitType.Star);
            HomeAddingPanel.Content = null;

            // Cancel previous operations
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();

            // Set SearchingStatusControl
            HomeOptionsBarSearchingStatusControl.Content = HomeOptionsBarSearchingStatusProgressRing;

            // Parse url
            (PlaylistSource Type, string ID) source = Source.GetPlaylistSource(e.Url);

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
                    await playlistService.GetVideosAsync(e.VideosCount, SearchingCancellationToken.Token);
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
                HomeOptionsBarSearchingStatusControl.Content = null;

                // Set UI
                HomeOptionBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                HomeTasksListRow.Height = new GridLength(0);
                
                // Open adding panel
                HomePlaylistAddingPanel addingPanel = new HomePlaylistAddingPanel(playlistService);
                addingPanel.TasksAddingRequested += HomeTasksAddingRequest;
                HomeAddingPanel.Content = addingPanel;
            }
        }


        // TASK ADDING REQUEST
        private void HomeTasksAddingRequest(object sender, TasksAddingRequestedEventArgs e)
        {
            // Replace placeholder
            HomeTasksListCurrentParent.Content = HomeTasksList;

            // Uncheck button
            switch (e.RequestSource)
            {
                case TaskAddingRequestSource.Video: HomeOptionsBarAddVideoButton.IsChecked = false; break;
                case TaskAddingRequestSource.Playlist: HomeOptionsBarAddPlaylistButton.IsChecked = false; break;
            }
            
            // Create video tasks
            foreach (TaskData taskData in e.TaskData)
            {
                HomeTaskPanel taskPanel = new HomeTaskPanel(taskData);

                taskPanel.TaskRemovingRequested += (s, a) =>
                {
                    // Remove task from tasks lists
                    TasksList.Remove(taskPanel);
                    HomeTasksList.Children.Remove(taskPanel);
                    if (TasksList.Count <= 0) HomeTasksListCurrentParent.Content = HomeTasksListPlaceholder;
                };

                // Add task to tasks lists
                HomeTasksList.Children.Add(taskPanel);
                TasksList.Add(taskPanel);
            }
        }


        // TASK ADDING CANCELLED
        private void HomeSearchingCancelled()
        {
            // Cancel searching operations
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();

            // Set grid dimensions
            HomeOptionBarAndAddingPanelRow.Height = GridLength.Auto;
            HomeTasksListRow.Height = new GridLength(1, GridUnitType.Star);

            // Clear panels
            HomeAddingPanel.Content = null;
            HomeOptionsBarAddingControl.Content = null;
            HomeOptionsBarSearchingStatusControl.Content = null;
        }


        // ADDING BUTTONS UNCHECKED
        private void HomeOptionsBarAddingButtons_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeSearchingCancelled();
        }


        // DOWNLOAD ALL BUTTON CLICKED
        private async void HomeOptionsBarDownloadAllButton_Click(object sender, RoutedEventArgs e)
        {
            HomeTaskPanel[] idleTasks = TasksList.Where((HomeTaskPanel video) => video.Status == Core.Enums.TaskStatus.Idle).ToArray();
            if (idleTasks.Count() > 0)
            {
                bool delay = (bool)Config.GetValue("delay_task_when_queued_task_starts_on_metered_network");
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
                {
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
                }

                foreach (HomeTaskPanel videoPanel in idleTasks)
                {
                    await Task.Delay(10);

                    #pragma warning disable CS4014
                    videoPanel.Start(delay);
                    #pragma warning restore CS4014
                }
            }
        }

        #endregion



        #region METHODS

        // WAIT IN QUEUE
        public static async Task WaitInQueue(bool delayWhenOnMeteredConnection, CancellationToken token)
        {
            while ((TasksList.Where((HomeTaskPanel task) => task.Status == Core.Enums.TaskStatus.InProgress).Count() >= (int)Config.GetValue("max_active_video_task") || (delayWhenOnMeteredConnection && NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)) && !token.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        #endregion
    }
}
