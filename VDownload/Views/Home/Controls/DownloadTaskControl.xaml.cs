using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Extensions;
using VDownload.Core.Services;
using VDownload.Core.Structs;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home.Controls
{
    public sealed partial class DownloadTaskControl : UserControl
    {
        #region CONSTANTS

        private static readonly ResourceDictionary IconsRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };
        private static readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public DownloadTaskControl(DownloadTask downloadTask)
        {
            this.InitializeComponent();

            DownloadTask = downloadTask;
            DownloadTask.StatusChanged += UpdateStatus;

            ThumbnailImage = DownloadTask.Video.Thumbnail != null ? new BitmapImage { UriSource = DownloadTask.Video.Thumbnail } : (BitmapImage)ImagesRes["UnknownThumbnailImage"];

            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{DownloadTask.Video.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };

            TimeSpan newDuration = DownloadTask.Trim.End.Subtract(DownloadTask.Trim.Start);
            StringBuilder durationBuilder = new StringBuilder(newDuration.ToStringOptTHBaseMMSS());
            if (DownloadTask.Video.Duration > newDuration)
            {
                durationBuilder.Append($" ({DownloadTask.Trim.Start.ToStringOptTHBaseMMSS(DownloadTask.Trim.End)} - {DownloadTask.Trim.End.ToStringOptTHBaseMMSS(DownloadTask.Trim.Start)})");
            }
            Duration = durationBuilder.ToString();

            StringBuilder mediaTypeQualityBuilder = new StringBuilder(ResourceLoader.GetForCurrentView().GetString($"Base_MediaType_{DownloadTask.MediaType}Text"));
            if (DownloadTask.MediaType != MediaType.OnlyAudio)
            {
                mediaTypeQualityBuilder.Append($" ({DownloadTask.SelectedStream})");
            }
            MediaTypeQuality = mediaTypeQualityBuilder.ToString();

            File = DownloadTask.File.GetPath();

            UpdateStatus(this, DownloadTask.LastStatusChangedEventArgs);
        }

        #endregion



        #region PROPERTIES

        private DownloadTask DownloadTask { get; set; }

        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Duration { get; set; }
        private string MediaTypeQuality { get; set; }
        private string File { get; set; }

        #endregion



        #region EVENT HANDLERS

        private async void StartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadTask.Status != DownloadTaskStatus.Idle)
            {
                DownloadTask.CancellationTokenSource.Cancel();
            }
            else
            {
                bool delay = (bool)Config.GetValue("delay_task_when_queued_task_starts_on_metered_network");
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
                {
                    ContentDialogResult dialogResult = await new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Start_MeteredConnection_Title"),
                        Content = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Start_MeteredConnection_Content"),
                        PrimaryButtonText = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Start_MeteredConnection_StartWithDelayButtonText"),
                        SecondaryButtonText = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Start_MeteredConnection_StartWithoutDelayButtonText1"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("Base_CancelButtonText"),
                    }.ShowAsync();
                    switch (dialogResult)
                    {
                        case ContentDialogResult.Primary: delay = true; break;
                        case ContentDialogResult.Secondary: delay = false; break;
                        case ContentDialogResult.None: return;
                    }
                }
                await DownloadTask.Run(delay);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (DownloadTask.Status != DownloadTaskStatus.Idle)
            {
                DownloadTask.CancellationTokenSource.Cancel();
            }
            RemovingRequested.Invoke(this, EventArgs.Empty);
        }

        private async void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(DownloadTask.Video.Url);
        }

        private void UpdateStatus(object sender, DownloadTaskStatusChangedEventArgs e)
        {
            if (e.Status == DownloadTaskStatus.Idle || e.Status == DownloadTaskStatus.EndedSuccessfully || e.Status == DownloadTaskStatus.EndedUnsuccessfully)
            {
                StartStopButton.Icon = new SymbolIcon(Symbol.Download);
            }
            else
            {
                StartStopButton.Icon = new SymbolIcon(Symbol.Stop);
            }

            if (e.Status == DownloadTaskStatus.Scheduled)
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateScheduledIcon"];
                StateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Scheduled")} ({e.ScheduledFor.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern)} {e.ScheduledFor.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortTimePattern)})";
                StateProgressBar.Visibility = Visibility.Collapsed;
            }
            else if (e.Status == DownloadTaskStatus.Queued)
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateQueuedIcon"];
                StateText.Text = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Queued");
                StateProgressBar.Visibility = Visibility.Visible;
                StateProgressBar.IsIndeterminate = true;
            }
            else if (e.Status == DownloadTaskStatus.Downloading)
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateDownloadingIcon"];
                StateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Downloading")} ({Math.Round(e.DownloadingProgress)}%)";
                StateProgressBar.Visibility = Visibility.Visible;
                StateProgressBar.IsIndeterminate = false;
                StateProgressBar.Value = e.DownloadingProgress;
            }
            else if (e.Status == DownloadTaskStatus.Processing)
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateProcessingIcon"];
                StateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Processing")} ({Math.Round(e.ProcessingProgress)}%)";
                StateProgressBar.Visibility = Visibility.Visible;
                StateProgressBar.IsIndeterminate = false;
                StateProgressBar.Value = e.ProcessingProgress;
            }
            else if (e.Status == DownloadTaskStatus.Finalizing)
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateFinalizingIcon"];
                StateText.Text = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Finalizing");
                StateProgressBar.Visibility = Visibility.Visible;
                StateProgressBar.IsIndeterminate = true;
            }
            else if (e.Status == DownloadTaskStatus.EndedSuccessfully)
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateDoneIcon"];
                StateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Done")} ({e.ElapsedTime.ToStringOptTHBaseMMSS()})";
                StateProgressBar.Visibility = Visibility.Collapsed;

                if ((bool)Config.GetValue("show_notification_when_task_ended_successfully"))
                {
                    new ToastContentBuilder()
                        .AddText(ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Notification_EndedSuccessfully_Header"))
                        .AddText($"\"{Title}\" - {ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Notification_EndedSuccessfully_Description")}")
                        .Show();
                }
            }
            else if (e.Status == DownloadTaskStatus.EndedUnsuccessfully)
            {
                if (e.Exception is OperationCanceledException)
                {
                    StateIcon.Source = (SvgImageSource)IconsRes["StateCancelledIcon"];
                    StateText.Text = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Cancelled");
                    StateProgressBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    string errorInfo;
                    if (e.Exception is WebException)
                    {
                        if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable) errorInfo = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Error_InternetNotAvailable");
                        else throw e.Exception;
                    }
                    else
                    {
                        throw e.Exception;
                    }
                    StateIcon.Source = (SvgImageSource)IconsRes["StateErrorIcon"];
                    StateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Error")} ({errorInfo})";
                    StateProgressBar.Visibility = Visibility.Collapsed;

                    if ((bool)Config.GetValue("show_notification_when_task_ended_unsuccessfully"))
                    {
                        new ToastContentBuilder()
                            .AddText(ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Notification_EndedUnsuccessfully_Header"))
                            .AddText($"\"{Title}\" - {ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_Notification_EndedUnsuccessfully_Description")} ({errorInfo})")
                            .Show();
                    }
                }
            }
            else
            {
                StateIcon.Source = (SvgImageSource)IconsRes["StateIdleIcon"];
                StateText.Text = ResourceLoader.GetForCurrentView().GetString("Home_DownloadTaskControl_State_Idle");
                StateProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler RemovingRequested;

        #endregion
    }
}
