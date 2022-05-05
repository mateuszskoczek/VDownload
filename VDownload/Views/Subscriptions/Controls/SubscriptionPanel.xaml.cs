using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VDownload.Core.Services;
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

namespace VDownload.Views.Subscriptions.Controls
{
    public sealed partial class SubscriptionPanel : UserControl
    {
        #region CONSTRUCTORS

        public SubscriptionPanel(Subscription subscription, StorageFile subscriptionFile)
        {
            InitializeComponent();
            Subscription = subscription;
            SubscriptionFile = subscriptionFile;

            TitleTextBlock.Text = Subscription.Playlist.Name;
            SourceButton.Icon = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{Subscription.Playlist.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
        }

        #endregion



        #region PROPERTIES

        private Subscription Subscription { get; set; }
        private StorageFile SubscriptionFile { get; set; }

        #endregion



        #region PUBLIC METHODS

        public async Task UpdateNewVideosCounterAsync()
        {
            CountTextBlock.Text = (await Subscription.GetNewVideosAsync()).Length.ToString();
        }

        #endregion



        #region EVENT HANDLERS

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            await SubscriptionsCollectionManagement.DeleteSubscriptionFileAsync(SubscriptionFile);
            ((StackPanel)Parent).Children.Remove(this);
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            CountTextBlock.Text = ResourceLoader.GetForCurrentView().GetString("Subscriptions_SubscriptionPanel_CountTextBlock_SyncText");
            await Subscription.GetNewVideosAndUpdateAsync();
            await SubscriptionsCollectionManagement.UpdateSubscriptionFileAsync(Subscription, SubscriptionFile);
            CountTextBlock.Text = "0";
        }

        private async void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(Subscription.Playlist.Url);
        }

        #endregion
    }
}
