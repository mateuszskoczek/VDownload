using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using VDownload.Core.Services.Sources;
using VDownload.Views.Subscriptions.Controls;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Subscriptions
{
    public sealed partial class MainPage : Page
    {
        #region CONSTRUCTORS

        public MainPage()
        {
            InitializeComponent();
        }

        #endregion



        #region EVENT HANDLERS

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            (Subscription Subscription, StorageFile SubscriptionFile)[] subscriptions = await SubscriptionsCollectionManagement.GetSubscriptionsAsync();
            foreach((Subscription Subscription, StorageFile SubscriptionFile) subscription in subscriptions)
            {
                AddSubscriptionToList(subscription.Subscription, subscription.SubscriptionFile);
            }
        }

        private async void AddingButton_Click(object sender, RoutedEventArgs e)
        {
            async Task ShowDialog(string localErrorKey)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = ResourceLoader.GetForCurrentView("DialogResources").GetString("Subscription_Adding_Base_Title"),
                    Content = ResourceLoader.GetForCurrentView("DialogResources").GetString($"Subscription_Adding_{localErrorKey}_Content"),
                    CloseButtonText = ResourceLoader.GetForCurrentView("DialogResources").GetString("Base_CloseButtonText"),
                };
                await errorDialog.ShowAsync();
                AddingProgressRing.Visibility = Visibility.Collapsed;
                AddingButton.IsEnabled = true;
            }

            AddingProgressRing.Visibility = Visibility.Visible;
            AddingButton.IsEnabled = false;

            IPlaylist playlist = Source.GetPlaylist(AddingTextBox.Text); // TODO: Change name of class (method returns Playlist object, not source of playlist)

            if (playlist is null)
            {
                await ShowDialog("PlaylistNotFound");
                return;
            }
            else
            {
                try
                {
                    await playlist.GetMetadataAsync();
                    await playlist.GetVideosAsync();
                }
                catch (MediaNotFoundException)
                {
                    await ShowDialog("PlaylistNotFound");
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

            Subscription subscription = new Subscription(playlist);

            StorageFile subscriptionFile = null;
            try
            {
                subscriptionFile = await SubscriptionsCollectionManagement.CreateSubscriptionFileAsync(subscription);
            }
            catch (SubscriptionExistsException)
            {
                await ShowDialog("SubscriptionExists");
                return;
            }

            AddSubscriptionToList(subscription, subscriptionFile);

            AddingProgressRing.Visibility = Visibility.Collapsed;
            AddingButton.IsEnabled = true;
            AddingTextBox.Text = string.Empty;
        }

        #endregion



        #region PRIVATE METHODS

        private void AddSubscriptionToList(Subscription subscription, StorageFile subscriptionFile)
        {
            SubscriptionPanel subscriptionPanel = new SubscriptionPanel(subscription, subscriptionFile);
            SubscriptionsListStackPanel.Children.Add(subscriptionPanel);
            subscriptionPanel.UpdateNewVideosCounterAsync();
        }

        #endregion
    }
}
