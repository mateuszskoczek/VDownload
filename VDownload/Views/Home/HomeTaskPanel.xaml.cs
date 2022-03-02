using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home
{
    public sealed partial class HomeTaskPanel : UserControl
    {
        #region CONSTANTS

        ResourceDictionary IconsRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };
        ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public HomeTaskPanel(IVideoService videoService, MediaType mediaType, IBaseStream stream, TimeSpan trimStart, TimeSpan trimEnd, string filename, MediaFileExtension extension, StorageFolder location)
        {
            this.InitializeComponent();

            // Set video status
            TaskStatus = Core.Enums.TaskStatus.Idle;

            // Set video service object
            VideoService = videoService;

            // Create video cancellation token
            CancellationTokenSource = new CancellationTokenSource();

            // Set video options
            MediaType = mediaType;
            Stream = stream;
            TrimStart = trimStart;
            TrimEnd = trimEnd;
            Filename = filename;
            Extension = extension;
            Location = location;

            // Set metadata
            ThumbnailImage = VideoService.Thumbnail != null ? new BitmapImage { UriSource = VideoService.Thumbnail } : (BitmapImage)ImagesRes["UnknownThumbnailImage"];
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{VideoService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Title = VideoService.Title;
            Author = VideoService.Author;
            TimeSpan newDuration = TrimEnd.Subtract(TrimStart);
            Duration += $"{(Math.Floor(newDuration.TotalHours) > 0 ? $"{Math.Floor(newDuration.TotalHours):0}:" : "")}{newDuration.Minutes:00}:{newDuration.Seconds:00}";
            if (VideoService.Duration > newDuration) Duration += $" ({(Math.Floor(TrimStart.TotalHours) > 0 || Math.Floor(TrimEnd.TotalHours) > 0 ? $"{Math.Floor(TrimStart.TotalHours):0}:" : "")}{TrimStart.Minutes:00}:{TrimStart.Seconds:00} - {(Math.Floor(TrimStart.TotalHours) > 0 || Math.Floor(TrimEnd.TotalHours) > 0 ? $"{Math.Floor(TrimEnd.TotalHours):0}:" : "")}{TrimEnd.Minutes:00}:{TrimEnd.Seconds:00})";
            MediaTypeQuality += ResourceLoader.GetForCurrentView().GetString($"MediaType{MediaType}Text");
            if (MediaType != MediaType.OnlyAudio) MediaTypeQuality += $" ({Stream.Height}p{(Stream.FrameRate > 0 ? Stream.FrameRate.ToString() : "N/A")})";
            File += $@"{(Location != null ? Location.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload")}\{Filename}.{Extension.ToString().ToLower()}";
            
            // Set state controls
            HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateIdleIcon"];
            HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextIdle");
            HomeTaskPanelStateProgressBar.Visibility = Visibility.Collapsed;
        }

        #endregion



        #region PROPERTIES

        // VIDEO STATUS
        public Core.Enums.TaskStatus TaskStatus { get; set; }

        // TASK CANCELLATION TOKEN
        public CancellationTokenSource CancellationTokenSource { get; set; }

        // VIDEO SERVICE
        private IVideoService VideoService { get; set; }

        // VIDEO OPTIONS
        private MediaType MediaType { get; set; }
        private IBaseStream Stream { get; set; }
        private TimeSpan TrimStart { get; set; }
        private TimeSpan TrimEnd { get; set; }
        private string Filename { get; set; }
        private MediaFileExtension Extension { get; set; }
        private StorageFolder Location { get; set; }

        // VIDEO PANEL DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Duration { get; set; }
        private string MediaTypeQuality { get; set; }
        private string File { get; set; }

        #endregion



        #region METHODS

        public async Task Start()
        {
            // Change icon
            HomeTaskPanelStartStopButton.Icon = new SymbolIcon(Symbol.Stop);

            // Create cancellation token
            CancellationTokenSource = new CancellationTokenSource();

            // Set task status
            TaskStatus = Core.Enums.TaskStatus.Waiting;

            // Set state controls
            HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateWaitingIcon"];
            HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextWaiting");
            HomeTaskPanelStateProgressBar.Visibility = Visibility.Visible;
            HomeTaskPanelStateProgressBar.IsIndeterminate = true;

            // Wait in queue
            await HomeMain.WaitInQueue(CancellationTokenSource.Token);
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                // Set task status
                TaskStatus = Core.Enums.TaskStatus.InProgress;

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
                    VideoService.DownloadingStarted += (s, a) =>
                    {
                        HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDownloadingIcon"];
                        HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextDownloading")} (0%)";
                        HomeTaskPanelStateProgressBar.IsIndeterminate = false;
                        HomeTaskPanelStateProgressBar.Value = 0;
                    };
                    VideoService.DownloadingProgressChanged += (s, a) =>
                    {
                        HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDownloadingIcon"];
                        HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextDownloading")} ({a.ProgressPercentage}%)";
                        HomeTaskPanelStateProgressBar.Value = a.ProgressPercentage;
                    };
                    VideoService.ProcessingStarted += (s, a) =>
                    {
                        HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateProcessingIcon"];
                        HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextProcessing")} (0%)";
                        HomeTaskPanelStateProgressBar.Value = 0;
                    };
                    VideoService.ProcessingProgressChanged += (s, a) =>
                    {
                        HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateProcessingIcon"];
                        HomeTaskPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextProcessing")} ({a.ProgressPercentage}%)";
                        HomeTaskPanelStateProgressBar.Value = a.ProgressPercentage;
                    };

                    // Request extended session
                    ExtendedExecutionSession session = new ExtendedExecutionSession { Reason = ExtendedExecutionReason.Unspecified };
                    await session.RequestExtensionAsync();

                    // Start task
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    StorageFile tempOutputFile = await VideoService.DownloadAndTranscodeAsync(tempFolder, Stream, Extension, MediaType, TrimStart, TrimEnd, CancellationTokenSource.Token);

                    // Dispose session
                    session.Dispose();

                    // Cancel if requested
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Set state controls
                    HomeTaskPanelStateIcon.Source = (SvgImageSource)IconsRes["StateFinalizingIcon"];
                    HomeTaskPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeTaskPanelStateTextFinalizing");
                    HomeTaskPanelStateProgressBar.IsIndeterminate = true;

                    // Move to output location
                    StorageFile outputFile;
                    if (Location != null) outputFile = await Location.CreateFileAsync($"{Filename}.{Extension.ToString().ToLower()}", (bool)Config.GetValue("replace_output_file_if_exists") ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName);
                    else outputFile = await DownloadsFolder.CreateFileAsync($"{Filename}.{Extension.ToString().ToLower()}", (bool)Config.GetValue("replace_output_file_if_exists") ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName);
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
                    TaskStatus = Core.Enums.TaskStatus.Idle;

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
            }
        }

        #endregion



        #region EVENT HANDLERS VOIDS

        // SOURCE BUTTON CLICKED
        private async void HomeTaskPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(VideoService.VideoUrl);
        }

        // START STOP BUTTON CLICKED
        private async void HomeTaskPanelStartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskStatus == Core.Enums.TaskStatus.InProgress || TaskStatus == Core.Enums.TaskStatus.Waiting) CancellationTokenSource.Cancel();
            else await Start();
        }

        // REMOVE BUTTON CLICKED
        private void HomeTaskPanelRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (TaskStatus == Core.Enums.TaskStatus.InProgress || TaskStatus == Core.Enums.TaskStatus.Waiting) CancellationTokenSource.Cancel();
            TaskRemovingRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler TaskRemovingRequested;

        #endregion
    }
}
