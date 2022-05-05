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
using VDownload.Views.Home.Controls;
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
                HomeTasksListCurrentParent.Content = new HomeTasksListPlaceholder();
            }
        }


        // SEARCH VIDEO BUTTON CHECKED
        private void HomeOptionsBarVideoSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            // Uncheck playlist button
            HomeOptionsBarPlaylistSearchButton.IsChecked = false;

            // Create video adding control
            HomeOptionsBarVideoSearch homeOptionsBarSearchVideoControl = new HomeOptionsBarVideoSearch();
            homeOptionsBarSearchVideoControl.VideoSearchSuccessed += HomeOptionsBarVideoSearchControl_VideoSearchSuccessed;
            homeOptionsBarSearchVideoControl.SearchButtonClick += (s, a) =>
            {
                SearchingCancellationToken.Cancel();
                SearchingCancellationToken = new CancellationTokenSource();
                homeOptionsBarSearchVideoControl.CancellationToken = SearchingCancellationToken.Token;
            };
            HomeOptionsBarAddingControl.Content = homeOptionsBarSearchVideoControl;
        }

        // VIDEO SEARCH SUCCESSED
        private void HomeOptionsBarVideoSearchControl_VideoSearchSuccessed(object sender, VideoSearchSuccessedEventArgs e)
        {
            // Set UI
            HomeOptionBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
            HomeTasksListRow.Height = new GridLength(0);

            // Open adding panel
            HomeVideoAddingPanel addingPanel = new HomeVideoAddingPanel(e.Video);
            addingPanel.TasksAddingRequested += HomeTasksAddingRequest;
            HomeAddingPanel.Content = addingPanel;
        }


        // SEARCH PLAYLIST BUTTON CHECKED
        private void HomeOptionsBarPlaylistSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            // Uncheck video button
            HomeOptionsBarVideoSearchButton.IsChecked = false;

            // Create playlist adding control
            HomeOptionsBarPlaylistSearch homeOptionsBarPlaylistSearchControl = new HomeOptionsBarPlaylistSearch();
            homeOptionsBarPlaylistSearchControl.PlaylistSearchSuccessed += HomeOptionsBarPlaylistSearchControl_PlaylistSearchSuccessed;
            homeOptionsBarPlaylistSearchControl.SearchButtonClick += (s, a) =>
            {
                SearchingCancellationToken.Cancel();
                SearchingCancellationToken = new CancellationTokenSource();
                homeOptionsBarPlaylistSearchControl.CancellationToken = SearchingCancellationToken.Token;
            };
            HomeOptionsBarAddingControl.Content = homeOptionsBarPlaylistSearchControl;
        }

        // PLAYLIST SEARCH SUCCESSED
        private void HomeOptionsBarPlaylistSearchControl_PlaylistSearchSuccessed(object sender, PlaylistSearchSuccessedEventArgs e)
        {
            // Set UI
            HomeOptionBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
            HomeTasksListRow.Height = new GridLength(0);

            // Open adding panel
            HomePlaylistAddingPanel addingPanel = new HomePlaylistAddingPanel(e.PlaylistService);
            addingPanel.TasksAddingRequested += HomeTasksAddingRequest;
            HomeAddingPanel.Content = addingPanel;
        }


        // TASK ADDING REQUEST
        private void HomeTasksAddingRequest(object sender, TasksAddingRequestedEventArgs e)
        {
            // Replace placeholder
            HomeTasksListCurrentParent.Content = HomeTasksList;

            // Uncheck button
            switch (e.RequestSource)
            {
                case TaskAddingRequestSource.Video: HomeOptionsBarVideoSearchButton.IsChecked = false; break;
                case TaskAddingRequestSource.Playlist: HomeOptionsBarPlaylistSearchButton.IsChecked = false; break;
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
                    if (TasksList.Count <= 0) HomeTasksListCurrentParent.Content = new HomeTasksListPlaceholder();
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
                    await Task.Delay(1);

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
