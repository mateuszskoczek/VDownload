using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Exceptions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using VDownload.Core.Services.Sources;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home.Controls
{
    public sealed partial class PlaylistSearchControl : UserControl
    {
        #region CONSTANTS

        private static readonly ResourceDictionary IconRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };

        private static readonly Microsoft.UI.Xaml.Controls.ProgressRing StatusControlProgressRing = new Microsoft.UI.Xaml.Controls.ProgressRing { Width = 15, Height = 15, Margin = new Thickness(15, 5, 5, 5), IsActive = true };
        private static readonly Image StatusControlErrorImage = new Image { Width = 15, Height = 15, Margin = new Thickness(15, 5, 5, 5), Source = (SvgImageSource)IconRes["ErrorIcon"] };

        #endregion



        #region CONSTRUCTORS

        public PlaylistSearchControl()
        {
            this.InitializeComponent();

            // Set default max videos
            MaxVideosNumberBox.Value = (int)Config.GetValue("default_max_playlist_videos");
        }

        #endregion



        #region PROPERTIES

        public CancellationToken CancellationToken { get; set; }

        #endregion



        #region EVENT HANDLERS

        private void MaxVideosNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(MaxVideosNumberBox.Value)) MaxVideosNumberBox.Value = (int)Config.GetValue("default_max_playlist_videos");
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            async Task ShowDialog(string localErrorKey)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = ResourceLoader.GetForCurrentView("DialogResources").GetString("Home_OptionsBar_PlaylistSearchControl_Base_Title"),
                    Content = ResourceLoader.GetForCurrentView("DialogResources").GetString($"Home_OptionsBar_PlaylistSearchControl_{localErrorKey}_Content"),
                    CloseButtonText = ResourceLoader.GetForCurrentView("DialogResources").GetString("Base_CloseButtonText"),
                };
                await errorDialog.ShowAsync();
                StatusControlPresenter.Content = StatusControlErrorImage;
            }

            SearchButtonClicked?.Invoke(this, EventArgs.Empty);

            InfoBox.IsOpen = false;

            StatusControlPresenter.Content = StatusControlProgressRing;

            MaxVideosNumberBox_LostFocus(sender, e);
            IPlaylist playlist = Source.GetPlaylist(UrlTextBox.Text);
            if (playlist == null)
            {
                StatusControlPresenter.Content = StatusControlErrorImage;
            }
            else
            {
                try
                {
                    await playlist.GetMetadataAsync(CancellationToken);
                    await playlist.GetVideosAsync((int)Math.Round(MaxVideosNumberBox.Value), CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    StatusControlPresenter.Content = null;
                    return;
                }
                catch (MediaNotFoundException)
                {
                    StatusControlPresenter.Content = StatusControlErrorImage;
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

                StatusControlPresenter.Content = null;
                
                SearchingSuccessed?.Invoke(this, new PlaylistSearchSuccessedEventArgs(playlist));
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            InfoBox.IsOpen = !InfoBox.IsOpen;
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<PlaylistSearchSuccessedEventArgs> SearchingSuccessed;
        public event EventHandler SearchButtonClicked;

        #endregion
    }
}
