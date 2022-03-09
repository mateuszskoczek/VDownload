using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
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
    public sealed partial class HomeTaskPanel : UserControl
    {
        #region CONSTANTS

        private static readonly ResourceDictionary IconsRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };
        private static readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public HomeTaskPanel(TaskData taskData)
        {
            this.InitializeComponent();

            // Set task data
            Data = taskData;

            // Set video status
            Status = Core.Enums.TaskStatus.Idle;

            // Create video cancellation token
            CancellationTokenSource = new CancellationTokenSource();

            // Set thumbnail image
            ThumbnailImage = Data.VideoService.Metadata.Thumbnail != null ? new BitmapImage { UriSource = Data.VideoService.Metadata.Thumbnail } : (BitmapImage)ImagesRes["UnknownThumbnailImage"];

            // Set source icon
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{Data.VideoService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };

            // Set duration
            TimeSpan newDuration = Data.TaskOptions.TrimEnd.Subtract(Data.TaskOptions.TrimStart);
            Duration = TimeSpanCustomFormat.ToOptTHBaseMMSS(newDuration);
            if (Data.VideoService.Metadata.Duration > newDuration) Duration += $" ({TimeSpanCustomFormat.ToOptTHBaseMMSS(Data.TaskOptions.TrimStart, Data.TaskOptions.TrimEnd)} - {TimeSpanCustomFormat.ToOptTHBaseMMSS(Data.TaskOptions.TrimEnd, Data.TaskOptions.TrimStart)})";

            // Set media type
            MediaTypeQuality += ResourceLoader.GetForCurrentView().GetString($"MediaType{Data.TaskOptions.MediaType}Text");
            if (Data.TaskOptions.MediaType != MediaType.OnlyAudio) MediaTypeQuality += $" ({Data.TaskOptions.Stream.Height}p{(Data.TaskOptions.Stream.FrameRate > 0 ? Data.TaskOptions.Stream.FrameRate.ToString() : "N/A")})";

            // Set file
            File += $@"{(Data.TaskOptions.Location != null ? Data.TaskOptions.Location.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload")}\{Data.TaskOptions.Filename}.{Data.TaskOptions.Extension.ToString().ToLower()}";
            
            // Set state controls
            HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateIdleIcon"];
            HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextIdle");
            HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;
        }

        #endregion



        #region PROPERTIES

        // TASK STATUS
        public Core.Enums.TaskStatus Status { get; set; }

        // TASK CANCELLATION TOKEN
        public CancellationTokenSource CancellationTokenSource { get; set; }

        // TASK DATA
        private TaskData Data { get; set; }

        // TASK PANEL DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Duration { get; set; }
        private string MediaTypeQuality { get; set; }
        private string File { get; set; }

        #endregion



        #region METHODS

        public async Task Start(bool delayWhenOnMeteredConnection)
        {
            // Change icon
            HomeTaskPanelStartStopButton.Icon = new SymbolIcon(Symbol.Stop);

            // Create cancellation token
            CancellationTokenSource = new CancellationTokenSource();

            // Scheduling
            if (Data.TaskOptions.Schedule > 0)
            {
                DateTime ScheduledDateTime = DateTime.Now.AddMinutes(Data.TaskOptions.Schedule);

                // Set task status
                Status = Core.Enums.TaskStatus.Scheduled;

                // Set state controls
                HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateScheduledIcon"];
                HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextScheduled")} ({ScheduledDateTime.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern)} {ScheduledDateTime.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortTimePattern)})";
                HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;

                while (DateTime.Now < ScheduledDateTime && !CancellationTokenSource.IsCancellationRequested) await Task.Delay(100);
            }

            // Set task status
            Status = Core.Enums.TaskStatus.Waiting;

            // Set state controls
            HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateWaitingIcon"];
            HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextWaiting");
            HomeTaskPanelStateProgressBar.Visibility = Visibility.Visible;

            // Wait in queue
            await HomeMain.WaitInQueue(delayWhenOnMeteredConnection, CancellationTokenSource.Token);
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                // Set task status
                Status = Core.Enums.TaskStatus.InProgress;

                // Get task unique ID
                string uniqueID = TaskId.Get();

                // Get temporary folder
                StorageFolder tempFolder;
                if ((bool)Config.GetValue("custom_temp_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_temp_location"))
                    tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("custom_temp_location");
                else
                    tempFolder = ApplicationData.Current.TemporaryFolder;
                tempFolder = await tempFolder.CreateFolderAsync(uniqueID);

                bool endedWithError = false;

                try
                {
                    // Throw exception if cancellation requested
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Start stopwatch
                    Stopwatch taskStopwatch = Stopwatch.StartNew();

                    // Set progress event handlers
                    Data.VideoService.DownloadingProgressChanged += (s, a) =>
                    {
                        HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDownloadingIcon"];
                        HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextDownloading")} ({Math.Round(a.Progress)}%)";
                        HomeTaskPanelStateProgressBar.Value = a.Progress;
                    };
                    Data.VideoService.ProcessingProgressChanged += (s, a) =>
                    {
                        HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateProcessingIcon"];
                        HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextProcessing")} ({Math.Round(a.Progress)}%)";
                        HomeTaskPanelStateProgressBar.Value = a.Progress;
                    };

                    // Request extended session
                    ExtendedExecutionSession session = new ExtendedExecutionSession { Reason = ExtendedExecutionReason.Unspecified };
                    await session.RequestExtensionAsync();

                    // Start task
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    StorageFile tempOutputFile = await Data.VideoService.DownloadAndTranscodeAsync(tempFolder, Data.TaskOptions.Stream, Data.TaskOptions.Extension, Data.TaskOptions.MediaType, Data.TaskOptions.TrimStart, Data.TaskOptions.TrimEnd, CancellationTokenSource.Token);

                    // Dispose session
                    session.Dispose();

                    // Cancel if requested
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Set state controls
                    HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateFinalizingIcon"];
                    HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextFinalizing");
                    HomeTaskPanelStateProgressBar.IsIndeterminate = true;

                    // Move to output location
                    string filename = $"{Data.TaskOptions.Filename}.{Data.TaskOptions.Extension.ToString().ToLower()}";
                    CreationCollisionOption collisionOption = (bool)Config.GetValue("replace_output_file_if_exists") ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName;
                    StorageFile outputFile = await (Data.TaskOptions.Location != null ? Data.TaskOptions.Location.CreateFileAsync(filename, collisionOption): DownloadsFolder.CreateFileAsync(filename, collisionOption));
                    await tempOutputFile.MoveAndReplaceAsync(outputFile);

                    // Stop stopwatch
                    taskStopwatch.Stop();

                    // Set state controls
                    HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDoneIcon"];
                    HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextDone")} ({(Math.Floor(taskStopwatch.Elapsed.TotalHours) > 0 ? $"{ Math.Floor(taskStopwatch.Elapsed.TotalHours):0}:" : "")}{taskStopwatch.Elapsed.Minutes:00}:{taskStopwatch.Elapsed.Seconds:00})";
                    HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;

                    // Show notification
                    if ((bool)Config.GetValue("show_notification_when_task_ended_successfully"))
                        new ToastContentBuilder()
                            .AddText(ResourceLoader.GetForCurrentView().GetString("NotificationTaskEndedSuccessfullyHeader"))
                            .AddText($"\"{Title}\" - {ResourceLoader.GetForCurrentView().GetString("NotificationTaskEndedSuccessfullyDescription")}")
                            .Show();
                }
                catch (OperationCanceledException)
                {
                    // Set state controls
                    HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateCancelledIcon"];
                    HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextCancelled");
                    HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;
                }
                catch (WebException ex)
                {
                    string errorInfo;
                    if (ex.Response is null) errorInfo = ResourceLoader.GetForCurrentView().GetString("TaskErrorInternetConnection");
                    else throw ex;

                    // Set state controls
                    HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateErrorIcon"];
                    HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextError")} ({errorInfo})";
                    HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;

                    // Show notification
                    if ((bool)Config.GetValue("show_notification_when_task_ended_unsuccessfully"))
                        new ToastContentBuilder()
                            .AddText(ResourceLoader.GetForCurrentView().GetString("NotificationTaskEndedUnsuccessfullyHeader"))
                            .AddText($"\"{Title}\" - {ResourceLoader.GetForCurrentView().GetString("NotificationTaskEndedUnsuccessfullyDescription")} ({errorInfo})")
                            .Show();
                }
                finally
                {
                    // Set video status
                    Status = Core.Enums.TaskStatus.Idle;

                    // Change icon
                    HomeTaskPanelStartStopButton.Icon = new SymbolIcon(Symbol.Download);

                    if (!endedWithError || (bool)Config.GetValue("delete_task_temp_when_ended_with_error"))
                    {
                        // Delete temporary files
                        await tempFolder.DeleteAsync();

                        // Dispose unique id
                        TaskId.Dispose(uniqueID);
                    }

                    if (!endedWithError && !CancellationTokenSource.IsCancellationRequested && (bool)Config.GetValue("remove_task_when_successfully_ended"))
                    {
                        // Remove task when successfully ended
                        TaskRemovingRequested?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                // Set state controls
                HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateCancelledIcon"];
                HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextCancelled");
                HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;

                // Change icon
                HomeTaskPanelStartStopButton.Icon = new SymbolIcon(Symbol.Download);
            }
        }

        #endregion



        #region EVENT HANDLERS VOIDS

        // SOURCE BUTTON CLICKED
        private async void HomeTaskPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(Data.VideoService.VideoUrl);
        }

        // START STOP BUTTON CLICKED
        private async void HomeTaskPanelStartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Status == Core.Enums.TaskStatus.InProgress || Status == Core.Enums.TaskStatus.Waiting || Status == Core.Enums.TaskStatus.Scheduled) CancellationTokenSource.Cancel();
            else
            {
                bool delay = (bool)Config.GetValue("delay_task_when_queued_task_starts_on_metered_network");
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
                {
                    ContentDialogResult dialogResult = await new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelTaskStartMeteredConnectionDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelTaskStartMeteredConnectionDialogDescription"),
                        PrimaryButtonText = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelTaskStartMeteredConnectionDialogStartAndDelayText"),
                        SecondaryButtonText = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelTaskStartMeteredConnectionDialogStartWithoutDelayText"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelTaskStartMeteredConnectionDialogCancel"),
                    }.ShowAsync();
                    switch (dialogResult)
                    {
                        case ContentDialogResult.Primary: delay = true; break;
                        case ContentDialogResult.Secondary: delay = false; break;
                        case ContentDialogResult.None: return;
                    }
                }
                await Start(delay);
            }
        }

        // REMOVE BUTTON CLICKED
        private void HomeTaskPanelRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Status == Core.Enums.TaskStatus.InProgress || Status == Core.Enums.TaskStatus.Waiting || Status == Core.Enums.TaskStatus.Scheduled) CancellationTokenSource.Cancel();
            TaskRemovingRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler TaskRemovingRequested;

        #endregion
    }
}
