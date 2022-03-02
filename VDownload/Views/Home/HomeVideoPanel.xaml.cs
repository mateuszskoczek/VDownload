using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using VDownload.Core.Objects;
using VDownload.Core.Services;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VDownload.Views.Home
{
    public sealed partial class HomeVideoPanel : UserControl
    {
        #region CONSTANTS

        ResourceDictionary IconsRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Icons.xaml") };

        #endregion



        #region CONSTRUCTORS

        public HomeVideoPanel(IVideoService videoService, MediaType mediaType, Stream stream, TimeSpan trimStart, TimeSpan trimEnd, string filename, MediaFileExtension extension, StorageFolder location)
        {
            this.InitializeComponent();

            // Set video status
            VideoStatus = VideoStatus.Idle;

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
            ThumbnailImage = new BitmapImage { UriSource = VideoService.Thumbnail ?? new Uri("ms-appx:///Assets/UnknownThumbnail.png") };
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Icons/{VideoService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Title = VideoService.Title;
            Author = VideoService.Author;
            TimeSpan newDuration = TrimEnd.Subtract(TrimStart);
            Duration += $"{(Math.Floor(newDuration.TotalHours) > 0 ? $"{Math.Floor(newDuration.TotalHours):0}:" : "")}{newDuration.Minutes:00}:{newDuration.Seconds:00}";
            if (VideoService.Duration > newDuration) Duration += $" ({(Math.Floor(TrimStart.TotalHours) > 0 || Math.Floor(TrimEnd.TotalHours) > 0 ? $"{Math.Floor(TrimStart.TotalHours):0}:" : "")}{TrimStart.Minutes:00}:{TrimStart.Seconds:00} - {(Math.Floor(TrimStart.TotalHours) > 0 || Math.Floor(TrimEnd.TotalHours) > 0 ? $"{Math.Floor(TrimEnd.TotalHours):0}:" : "")}{TrimEnd.Minutes:00}:{TrimEnd.Seconds:00})";
            MediaTypeQuality += ResourceLoader.GetForCurrentView().GetString($"MediaType{MediaType}Text");
            if (MediaType != MediaType.OnlyAudio) MediaTypeQuality += $" ({Stream.Height}p{(Stream.FrameRate > 0 ? Stream.FrameRate.ToString() : "N/A")})";
            File += $@"{(Location != null ? Location.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload")}\{Filename}.{Extension.ToString().ToLower()}";
            
            // Set state controls
            HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateIdleIcon"];
            HomeVideoPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextIdle");
            HomeVideoPanelStateProgressBar.Visibility = Visibility.Collapsed;
        }

        #endregion



        #region PROPERTIES

        // VIDEO STATUS
        public VideoStatus VideoStatus { get; set; }

        // VIDEO CANECELLATION TOKEN
        public CancellationTokenSource CancellationTokenSource { get; set; }

        // VIDEO SERVICE
        private IVideoService VideoService { get; set; }

        // VIDEO OPTIONS
        private MediaType MediaType { get; set; }
        private Stream Stream { get; set; }
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
            HomeVideoPanelStartStopButton.Icon = new SymbolIcon(Symbol.Stop);

            // Create cancellation token
            CancellationTokenSource = new CancellationTokenSource();

            // Set video status
            VideoStatus = VideoStatus.Waiting;

            // Set state controls
            HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateWaitingIcon"];
            HomeVideoPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextWaiting");
            HomeVideoPanelStateProgressBar.Visibility = Visibility.Visible;
            HomeVideoPanelStateProgressBar.IsIndeterminate = true;

            // Wait in queue
            await HomeMain.WaitInQueue(CancellationTokenSource.Token);
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                // Set video status
                VideoStatus = VideoStatus.InProgress;

                // Get task unique ID
                string uniqueID = TaskId.Get();

                // Get temporary folder
                StorageFolder tempFolder;
                if ((bool)Config.GetValue("custom_temp_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_temp_location"))
                    tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("custom_temp_location");
                else
                    tempFolder = ApplicationData.Current.TemporaryFolder;
                tempFolder = await tempFolder.CreateFolderAsync(uniqueID);

                try
                {
                    // Set cancellation token to throw exception on request
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Start stopwatch
                    Stopwatch taskStopwatch = Stopwatch.StartNew();

                    // Set progress event handlers
                    VideoService.DownloadingStarted += (s, a) =>
                    {
                        HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDownloadingIcon"];
                        HomeVideoPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextDownloading")} (0%)";
                        HomeVideoPanelStateProgressBar.IsIndeterminate = false;
                        HomeVideoPanelStateProgressBar.Value = 0;
                    };
                    VideoService.DownloadingProgressChanged += (s, a) =>
                    {
                        HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDownloadingIcon"];
                        HomeVideoPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextDownloading")} ({a.ProgressPercentage}%)";
                        HomeVideoPanelStateProgressBar.Value = a.ProgressPercentage;
                    };
                    VideoService.ProcessingStarted += (s, a) =>
                    {
                        HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateProcessingIcon"];
                        HomeVideoPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextProcessing")} (0%)";
                        HomeVideoPanelStateProgressBar.Value = 0;
                    };
                    VideoService.ProcessingProgressChanged += (s, a) =>
                    {
                        HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateProcessingIcon"];
                        HomeVideoPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextProcessing")} ({a.ProgressPercentage}%)";
                        HomeVideoPanelStateProgressBar.Value = a.ProgressPercentage;
                    };

                    // Request extended session
                    ExtendedExecutionSession session = new ExtendedExecutionSession { Reason = ExtendedExecutionReason.Unspecified };
                    await session.RequestExtensionAsync();

                    // Start task
                    StorageFile tempOutputFile = await VideoService.DownloadAndTranscodeAsync(tempFolder, Stream, Extension, MediaType, CancellationTokenSource.Token);

                    // Dispose session
                    session.Dispose();

                    // Cancel if requested
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    // Set state controls
                    HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateFinalizingIcon"];
                    HomeVideoPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextFinalizing");
                    HomeVideoPanelStateProgressBar.IsIndeterminate = true;

                    // Move to output location
                    StorageFile outputFile;
                    Debug.WriteLine($"{Filename}.{Extension.ToString().ToLower()}");
                    if (Location != null) outputFile = await Location.CreateFileAsync($"{Filename}.{Extension.ToString().ToLower()}", (bool)Config.GetValue("replace_output_file_if_exists") ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName);
                    else outputFile = await DownloadsFolder.CreateFileAsync($"{Filename}.{Extension.ToString().ToLower()}", (bool)Config.GetValue("replace_output_file_if_exists") ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.GenerateUniqueName);
                    await tempOutputFile.MoveAndReplaceAsync(outputFile);

                    // Stop stopwatch
                    taskStopwatch.Stop();

                    // Set state controls
                    HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateDoneIcon"];
                    HomeVideoPanelStateText.Text = $"{ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextDone")} ({(Math.Floor(taskStopwatch.Elapsed.TotalHours) > 0 ? $"{ Math.Floor(taskStopwatch.Elapsed.TotalHours):0}:" : "")}{taskStopwatch.Elapsed.Minutes:00}:{taskStopwatch.Elapsed.Seconds:00})";
                    HomeVideoPanelStateProgressBar.Visibility = Visibility.Collapsed;

                }
                catch (OperationCanceledException)
                {
                    // Set state controls
                    HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateCancelledIcon"];
                    HomeVideoPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextCancelled");
                    HomeVideoPanelStateProgressBar.Visibility = Visibility.Collapsed;
                }
                finally
                {
                    // Change icon
                    HomeVideoPanelStartStopButton.Icon = new SymbolIcon(Symbol.Download);

                    // Set video status
                    VideoStatus = VideoStatus.Idle;

                    // Delete temporary files
                    await tempFolder.DeleteAsync();

                    // Dispose unique id
                    TaskId.Dispose(uniqueID);
                }
            }
            else
            {
                // Set state controls
                HomeVideoPanelStateIcon.Source = (SvgImageSource)IconsRes["StateCancelledIcon"];
                HomeVideoPanelStateText.Text = ResourceLoader.GetForCurrentView().GetString("HomeVideoPanelStateTextCancelled");
                HomeVideoPanelStateProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        #endregion



        #region EVENT HANDLERS VOIDS

        // SOURCE BUTTON CLICKED
        private async void HomeVideoPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(VideoService.VideoUrl);
        }

        // START STOP BUTTON CLICKED
        private async void HomeVideoPanelStartStopButton_Click(object sender, RoutedEventArgs e)
        {
            if (VideoStatus == VideoStatus.InProgress || VideoStatus == VideoStatus.Waiting) CancellationTokenSource.Cancel();
            else await Start();
        }

        // REMOVE BUTTON CLICKED
        private void HomeVideoPanelRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (VideoStatus == VideoStatus.InProgress || VideoStatus == VideoStatus.Waiting) CancellationTokenSource.Cancel();
            VideoRemovingRequested?.Invoke(sender, EventArgs.Empty);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler VideoRemovingRequested;

        #endregion
    }
}
