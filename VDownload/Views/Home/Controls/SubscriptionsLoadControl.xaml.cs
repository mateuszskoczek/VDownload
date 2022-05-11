using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using VDownload.Core.Services.Sources;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home.Controls
{
    public sealed partial class SubscriptionsLoadControl : UserControl
    {
        #region CONSTRUCTORS

        public SubscriptionsLoadControl()
        {
            this.InitializeComponent();

            InfoTextBlock.Text = ResourceLoader.GetForCurrentView().GetString("Home_OptionsBar_SubscriptionsLoadControl_InfoTextBlock_Loading");
        }

        #endregion



        #region PROPERTIES

        public CancellationToken CancellationToken { get; set; }

        #endregion



        #region PUBLIC METHODS

        public async Task StartLoading()
        {
            List<IVideo> loadedVideos = new List<IVideo>();
            (Subscription Subscription, StorageFile SubscriptionFile)[] subscriptions = await SubscriptionsCollectionManagement.GetSubscriptionsAsync();
            foreach ((Subscription Subscription, StorageFile SubscriptionFile) subscription in subscriptions)
            {
                try
                {
                    loadedVideos.AddRange(await subscription.Subscription.GetNewVideosAsync(CancellationToken));
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (MediaNotFoundException)
                {
                    await ShowDialog("MediaNotFound", subscription.Subscription.Playlist.Name);
                    return;
                }
                catch (TwitchAccessTokenNotFoundException)
                {
                    await ShowDialog("TwitchAccessTokenNotFound");
                    return;
                }
                catch (TwitchAccessTokenNotValidException)
                {
                    await ShowDialog("TwitchAccessTokenNotValid");
                    return;
                }
                catch (WebException)
                {
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        await ShowDialog("InternetNotAvailable");
                        return;
                    }
                    else throw;
                }
            }

            ProgressRing.Visibility = Visibility.Collapsed;
            InfoTextBlock.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_OptionsBar_SubscriptionsLoadControl_InfoTextBlock_Found")}: {loadedVideos.Count}";
            if (loadedVideos.Count > 0)
            {
                LoadVideosButton.Visibility = Visibility.Visible;
            }
        }

        #endregion



        #region EVENT HANDLERS

        private async void LoadVideosButton_Click(object sender, RoutedEventArgs e)
        {
            ProgressRing.Visibility = Visibility.Visible;
            LoadVideosButton.IsEnabled = false;

            List<IVideo> loadedVideos = new List<IVideo>();
            (Subscription Subscription, StorageFile SubscriptionFile)[] subscriptions = await SubscriptionsCollectionManagement.GetSubscriptionsAsync();
            foreach ((Subscription Subscription, StorageFile SubscriptionFile) subscription in subscriptions)
            {
                try
                {
                    loadedVideos.AddRange(await subscription.Subscription.GetNewVideosAndUpdateAsync(CancellationToken));
                    await SubscriptionsCollectionManagement.UpdateSubscriptionFileAsync(subscription.Subscription, subscription.SubscriptionFile);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (MediaNotFoundException)
                {
                    await ShowDialog("MediaNotFound", subscription.Subscription.Playlist.Name);
                    return;
                }
                catch (TwitchAccessTokenNotFoundException)
                {
                    await ShowDialog("TwitchAccessTokenNotFound");
                    return;
                }
                catch (TwitchAccessTokenNotValidException)
                {
                    await ShowDialog("TwitchAccessTokenNotValid");
                    return;
                }
                catch (WebException)
                {
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        await ShowDialog("InternetNotAvailable");
                        return;
                    }
                    else throw;
                }
            }

            LoadingSuccessed.Invoke(this, new SubscriptionLoadSuccessedEventArgs(loadedVideos.ToArray()));
        }

        #endregion



        #region PRIVATE METHODS

        private async Task ShowDialog(string localErrorKey, string playlistName = null)
        {
            StringBuilder content = new StringBuilder(ResourceLoader.GetForCurrentView("DialogResources").GetString($"Home_OptionsBar_SubscriptionsLoadControl_{localErrorKey}_Content"));
            if (!string.IsNullOrEmpty(playlistName))
            {
                content.Append($" {playlistName}");
            }
            ContentDialog errorDialog = new ContentDialog
            {
                Title = ResourceLoader.GetForCurrentView("DialogResources").GetString("Home_OptionsBar_SubscriptionsLoadControl_Base_Title"),
                Content = content.ToString(),
                CloseButtonText = ResourceLoader.GetForCurrentView("DialogResources").GetString("Base_CloseButtonText"),
            };
            await errorDialog.ShowAsync();
            ProgressRing.Visibility = Visibility.Collapsed;
            InfoTextBlock.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_OptionsBar_SubscriptionsLoadControl_InfoTextBlock_Found")}: 0";
        }

        #endregion



        #region EVENTS

        public event EventHandler<SubscriptionLoadSuccessedEventArgs> LoadingSuccessed;

        #endregion
    }
}
