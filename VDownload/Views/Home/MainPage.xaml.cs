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
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Home
{
    public sealed partial class MainPage : Page
    {
        #region CONSTRUCTORS

        public MainPage()
        {
            this.InitializeComponent();

            SearchingCancellationToken = new CancellationTokenSource();
        }

        #endregion



        #region PROPERTIES

        private CancellationTokenSource SearchingCancellationToken { get; set; }

        #endregion



        #region EVENT HANDLERS

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            foreach (DownloadTask downloadTask in DownloadTasksCollectionManagement.DownloadTasksCollection.Values)
            {
                DownloadTaskControl downloadTaskControl = new DownloadTaskControl(downloadTask);
                downloadTaskControl.RemovingRequested += (s, a) =>
                {
                    DownloadTaskControlsStackPanel.Remove(downloadTaskControl);
                    DownloadTasksCollectionManagement.Remove(downloadTask.Id);
                };
                DownloadTaskControlsStackPanel.Add(downloadTaskControl);
            }
        }

        private void VideoSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            PlaylistSearchButton.IsChecked = false;
            LoadSubscripionsButton.IsChecked = false;

            VideoSearchControl videoSearchControl = new VideoSearchControl();
            videoSearchControl.SearchingSuccessed += (s, a) =>
            {
                OptionsBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                DownloadTaskControlsStackPanelRow.Height = new GridLength(0);

                VideoAddingPanel videoAddingPanel = new VideoAddingPanel(a.Video);
                videoAddingPanel.TasksAddingRequested += DownloadTasksAddingRequest;
                AddingPanelPresenter.Content = videoAddingPanel;
            };
            videoSearchControl.SearchButtonClicked += (s, a) =>
            {
                SearchingCancellationToken.Cancel();
                SearchingCancellationToken = new CancellationTokenSource();
                videoSearchControl.CancellationToken = SearchingCancellationToken.Token;
            };
            SearchControlPresenter.Content = videoSearchControl;
        }

        private void PlaylistSearchButton_Checked(object sender, RoutedEventArgs e)
        {
            VideoSearchButton.IsChecked = false;
            LoadSubscripionsButton.IsChecked = false;

            PlaylistSearchControl playlistSearchControl = new PlaylistSearchControl();
            playlistSearchControl.SearchingSuccessed += (s, a) =>
            {
                OptionsBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                DownloadTaskControlsStackPanelRow.Height = new GridLength(0);

                PlaylistAddingPanel playlistAddingPanel = new PlaylistAddingPanel(a.Playlist);
                playlistAddingPanel.TasksAddingRequested += DownloadTasksAddingRequest;
                AddingPanelPresenter.Content = playlistAddingPanel;
            };
            playlistSearchControl.SearchButtonClicked += (s, a) =>
            {
                SearchingCancellationToken.Cancel();
                SearchingCancellationToken = new CancellationTokenSource();
                playlistSearchControl.CancellationToken = SearchingCancellationToken.Token;
            };
            SearchControlPresenter.Content = playlistSearchControl;
        }

        private async void LoadSubscripionsButton_Checked(object sender, RoutedEventArgs e)
        {
            VideoSearchButton.IsChecked = false;
            PlaylistSearchButton.IsChecked = false;

            SubscriptionsLoadControl subscriptionsLoadControl = new SubscriptionsLoadControl();
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();
            subscriptionsLoadControl.CancellationToken = SearchingCancellationToken.Token;
            subscriptionsLoadControl.LoadingSuccessed += (s, a) =>
            {
                OptionsBarAndAddingPanelRow.Height = new GridLength(1, GridUnitType.Star);
                DownloadTaskControlsStackPanelRow.Height = new GridLength(0);

                SubscriptionsAddingPanel subscriptionsAddingPanel = new SubscriptionsAddingPanel(a.Videos);
                subscriptionsAddingPanel.TasksAddingRequested += DownloadTasksAddingRequest;
                AddingPanelPresenter.Content = subscriptionsAddingPanel;
            };
            SearchControlPresenter.Content = subscriptionsLoadControl;
            await subscriptionsLoadControl.StartLoading();
        }

        private void AddingButtons_Unchecked(object sender, RoutedEventArgs e)
        {
            SearchingCancellationToken.Cancel();
            SearchingCancellationToken = new CancellationTokenSource();

            OptionsBarAndAddingPanelRow.Height = GridLength.Auto;
            DownloadTaskControlsStackPanelRow.Height = new GridLength(1, GridUnitType.Star);

            AddingPanelPresenter.Content = null;
            SearchControlPresenter.Content = null;
            SearchingStatusControlPresenter.Content = null;
        }

        private void DownloadTasksAddingRequest(object sender, DownloadTasksAddingRequestedEventArgs e)
        {
            switch (e.RequestSource)
            {
                case DownloadTasksAddingRequestSource.Video: VideoSearchButton.IsChecked = false; break;
                case DownloadTasksAddingRequestSource.Playlist: PlaylistSearchButton.IsChecked = false; break;
                case DownloadTasksAddingRequestSource.Subscriptions: LoadSubscripionsButton.IsChecked = false; break;
            }
            
            foreach (DownloadTask downloadTask in e.DownloadTasks)
            { 
                DownloadTaskControl downloadTaskControl = new DownloadTaskControl(downloadTask);
                DownloadTasksCollectionManagement.Add(downloadTask, downloadTask.Id);
                downloadTaskControl.RemovingRequested += (s, a) =>
                {
                    DownloadTaskControlsStackPanel.Remove(downloadTaskControl);
                    DownloadTasksCollectionManagement.Remove(downloadTask.Id);
                };

                DownloadTaskControlsStackPanel.Add(downloadTaskControl);
            }
        }

        private async void DownloadAllButton_Click(object sender, RoutedEventArgs e)
        {
            DownloadTask[] idleDownloadTasks = DownloadTasksCollectionManagement.DownloadTasksCollection.Values.Where((task) => task.Status == DownloadTaskStatus.Idle).ToArray();
            if (idleDownloadTasks.Count() > 0)
            {
                bool delay = (bool)Config.GetValue("delay_task_when_queued_task_starts_on_metered_network");
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
                {
                    ContentDialogResult dialogResult = await new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView("DialogResources").GetString("Home_DownloadAll_MeteredConnection_Title"),
                        Content = ResourceLoader.GetForCurrentView("DialogResources").GetString("Home_DownloadAll_MeteredConnection_Content"),
                        PrimaryButtonText = ResourceLoader.GetForCurrentView("DialogResources").GetString("Home_DownloadAll_MeteredConnection_StartWithDelayButtonText"),
                        SecondaryButtonText = ResourceLoader.GetForCurrentView("DialogResources").GetString("Home_DownloadAll_MeteredConnection_StartWithoutDelayButtonText"),
                        CloseButtonText = ResourceLoader.GetForCurrentView("DialogResources").GetString("Base_CancelButtonText"),
                    }.ShowAsync();
                    switch (dialogResult)
                    {
                        case ContentDialogResult.Primary: delay = true; break;
                        case ContentDialogResult.Secondary: delay = false; break;
                        case ContentDialogResult.None: return;
                    }
                }

                foreach (DownloadTask downloadTask in idleDownloadTasks)
                {
                    await Task.Delay(1);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    downloadTask.Run(delay);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        #endregion
    }
}
