// Internal
using VDownload.Objects.Enums;
using VDownload.Services;

// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI;
using System.Diagnostics;

namespace VDownload.Sources
{
    public class VObject
    {
        #region INIT

        // VIDEO METADATA
        private string UniqueID { get; set; }
        public VideoSource SourceType { get; private set; }
        public string ID { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public DateTime Date { get; private set; }
        public long Views { get; private set; }
        public TimeSpan Duration { get; private set; }
        public Uri Url { get; private set; }
        public Uri Thumbnail { get; private set; }
        public Uri SourceIcon { get; private set; }
        public Dictionary<string, Uri> Streams { get; private set; }

        // FILE PROCESS DATA
        public string SelectedQuality { get; set; }
        public TimeSpan TrimStart { get; set; }
        public TimeSpan TrimEnd { get; set; }
        public string MediaType { get; set; }
        public string Filename { get; set; }
        public string Extension { get; set; }
        public string FilePath { get; set; }
        public StorageFolder CustomSaveLocation { get; set; }

        // VIDEO PANEL
        private Grid VideoPanel { get; set; }
        private StackPanel VideoPanelParent { get; set; }
        private AppBarButton StartStopButton { get; set; }
        private TextBlock ProgressLabelTextblock { get; set; }
        private ProgressBar ProgressBar { get; set; }
        private Image ProgressIcon { get; set; }

        // VIDEO TASK
        private object VideoSourceHandler { get; set; }
        private CancellationTokenSource VideoTaskCancellationToken { get; set; }
        private Task<StorageFile> VideoTask { get; set; }
        public VideoStatus VideoStatus { get; private set; }

        // CONSTRUCTOR
        public VObject(Uri url)
        {
            (SourceType, ID) = Source.GetVideoSourceData(url);
            switch (SourceType) // (TODO)
            {
                case VideoSource.TwitchVod:
                    VideoSourceHandler = new Twitch.Vod(ID);
                    SourceIcon = new Uri("ms-appx:///Assets/Icons/Sources/Twitch.png");
                    break;
                case VideoSource.TwitchClip:
                    VideoSourceHandler = new Twitch.Clip(ID);
                    SourceIcon = new Uri("ms-appx:///Assets/Icons/Sources/Twitch.png");
                    break;
            }
            UniqueID = Videos.GetUniqueID();
        }

        #endregion



        #region MAIN

        // GET METADATA AND SET DATA VARIABLES
        public async Task GetMetadata()
        {
            // Get metadata and streams
            Task<Dictionary<string, object>> metadataTask;
            Task<Dictionary<string, Uri>> streamsTask;
            switch (SourceType) // (TODO)
            {
                case VideoSource.TwitchVod:
                    metadataTask = ((Twitch.Vod)VideoSourceHandler).GetMetadata();
                    streamsTask = ((Twitch.Vod)VideoSourceHandler).GetStreams();
                    break;
                case VideoSource.TwitchClip:
                    metadataTask = ((Twitch.Clip)VideoSourceHandler).GetMetadata();
                    streamsTask = ((Twitch.Clip)VideoSourceHandler).GetStreams();
                    break;
                default:
                    throw new Exception(message: "Unknown video source");
            }
            await Task.WhenAll(metadataTask, streamsTask);
            Dictionary<string, object> metadata = metadataTask.Result;
            Dictionary<string, Uri> streams = streamsTask.Result;

            // Set metadata
            Title = (string)metadata["title"];
            Author = (string)metadata["author"];
            Date = (DateTime)metadata["date"];
            Views = (long)metadata["views"];
            Duration = (TimeSpan)metadata["duration"];
            Thumbnail = (Uri)metadata["thumbnail"];
            Url = (Uri)metadata["url"];
            Streams = streams;

            // Set default media type
            MediaType = Config.GetValue("default_media_type");

            // Set default quality
            SelectedQuality = Streams.Keys.ToArray()[0];

            // Set trim timestamps
            TrimStart = new TimeSpan(0);
            TrimEnd = Duration;

            // Set defualt filename
            Dictionary<string, string> filenameTemplate = new Dictionary<string, string>()
            {
                { "%title%", Title },
                { "%author%", Author },
                { "%date_pub%", Date.ToString(Config.GetValue("date_format")) },
                { "%date_now%", DateTime.Now.ToString(Config.GetValue("date_format")) },
                { "%views%", Views.ToString() },
                { "%id%", ID }
            };
            string temporaryFilename = Config.GetValue("default_output_filename");
            foreach (KeyValuePair<string, string> t in filenameTemplate)
            { temporaryFilename = temporaryFilename.Replace(t.Key, t.Value); }
            foreach (char c in Path.GetInvalidFileNameChars())
            { temporaryFilename = temporaryFilename.Replace(c, ' '); }
            Filename = temporaryFilename;

            // Set extension
            Extension = MediaType == "A" ? Config.GetValue("default_audio_extension") : Config.GetValue("default_video_extension");

            // Set visible path
            if (CustomSaveLocation != null) FilePath = $@"{CustomSaveLocation.Path}\{Filename}.{Extension.ToLower()}";
            else FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Filename}.{Extension.ToLower()}";
        }

        // ADD VIDEO TO LIST
        public void AddVideoToList(StackPanel parent)
        {
            // Set status to idle
            VideoStatus = VideoStatus.Idle;

            // Video panel management
            VideoPanel = CreateVideoPanel();
            VideoPanelParent = parent;
            VideoPanelParent.Children.Add(VideoPanel);

            // Add VObject to listed videos list
            Videos.VideoObjectsList.Add(this);
        }

        // REMOVE VIDEO FROM LIST
        public void RemoveVideoFromList()
        {
            // Remove video from video list
            VideoPanelParent.Children.Remove(VideoPanel);

            // Remove VObject from listed videos list
            Videos.VideoObjectsList.Remove(this);

            // Set status as removed
            VideoStatus = VideoStatus.Removed;
        }

        // START VIDEO TASK
        public async Task Start()
        {
            // Change video status to idle
            VideoStatus = VideoStatus.Waiting;

            // Set cancellation token
            VideoTaskCancellationToken = new CancellationTokenSource();

            // Set panel (start and waiting)
            StartStopButton.Icon = new SymbolIcon(Symbol.Stop);
            ProgressLabelTextblock.Text = ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelWaiting");
            ProgressBar.IsIndeterminate = true;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressIcon.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Waiting.png") };

            // Wait for free space in active video tasks list
            await Videos.WaitForFreeSpace(VideoTaskCancellationToken.Token);

            // Set end message
            string endMessage = ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelCancelled");
            Uri endIconSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Cancelled.png");

            // Start video downloading process
            if (!VideoTaskCancellationToken.IsCancellationRequested)
            {
                // Init temp folder
                StorageFolder tempFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync(UniqueID);
                
                // Set video task handler (TODO)
                switch (SourceType)
                {
                    case VideoSource.TwitchVod:
                        VideoTask = ((Twitch.Vod)VideoSourceHandler).Download(tempFolder, SelectedQuality, Extension, MediaType, TrimStart, TrimEnd, Duration, VideoTaskCancellationToken, ProgressLabelTextblock, ProgressBar, ProgressIcon);
                        break;
                    case VideoSource.TwitchClip:
                        VideoTask = ((Twitch.Clip)VideoSourceHandler).Download(tempFolder, SelectedQuality, Extension, MediaType, TrimStart, TrimEnd, Duration, VideoTaskCancellationToken, ProgressLabelTextblock, ProgressBar, ProgressIcon);
                        break;
                    default:
                        throw new Exception(message: "Unknown video source");
                }

                // Add video task to active video tasks list
                Videos.VideoTasksList.Add(VideoTask);
                VideoStatus = VideoStatus.InProgress;

                // Start video task
                StorageFile downloadedFile = null;
                bool error = false;
                string errorMessage = "";
                Stopwatch executeStopwatch = new Stopwatch();
                executeStopwatch.Start();
                try
                {
                    // Request no suspendable session
                    ExtendedExecutionSession session = new ExtendedExecutionSession { Reason = ExtendedExecutionReason.Unspecified };
                    await session.RequestExtensionAsync();

                    // Run task
                    downloadedFile = await VideoTask;

                    // Dispose session
                    session.Dispose();
                }
                catch (Exception ex)
                {
                    // Set error info (error identifier and endMessage)
                    error = true;
                    errorMessage = ex.Message;
                }
                finally
                {
                    // Set progress to finalizing
                    ProgressLabelTextblock.Text = ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelFinalizing");
                    ProgressBar.IsIndeterminate = true;
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressIcon.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Finalizing.png") };

                    // Move to output destination
                    if (!VideoTaskCancellationToken.IsCancellationRequested && !error)
                    {
                        // Set output file
                        StorageFile outputFile;
                        if (CustomSaveLocation != null) outputFile = await CustomSaveLocation.CreateFileAsync($"{Filename}.{Extension.ToLower()}", CreationCollisionOption.GenerateUniqueName);
                        else outputFile = await DownloadsFolder.CreateFileAsync($"{Filename}.{Extension.ToLower()}", CreationCollisionOption.GenerateUniqueName);

                        // Create and move file
                        await downloadedFile.MoveAndReplaceAsync(outputFile);
                    }

                    // Delete temp
                    if (!error || Config.GetValue("delete_video_temp_after_error") == "1") 
                        await tempFolder.DeleteAsync();
                    
                    // Remove video task from active video tasks list
                    Videos.VideoTasksList.Remove(VideoTask);
                    VideoStatus = VideoStatus.Idle;

                    // Stop stopwatch
                    executeStopwatch.Stop();

                    // Set end message
                    if (!VideoTaskCancellationToken.IsCancellationRequested)
                    {
                        if (error)
                        {
                            endMessage = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelError")} ({errorMessage})";
                            endIconSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Error.png");
                        }
                        else
                        {
                            endMessage = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelDone1")} {Math.Round(executeStopwatch.Elapsed.TotalSeconds, 0)} {ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelDone2")}";
                            endIconSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Done.png");
                        }
                    }
                }
            }

            // End message
            ProgressLabelTextblock.Text = endMessage;
            StartStopButton.Icon = new SymbolIcon(Symbol.Download);
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressIcon.Source = new BitmapImage { UriSource = endIconSource };
        }

        #endregion



        #region PANEL

        // CREATE VIDEO PANEL
        private Grid CreateVideoPanel()
        {
            // Base grid
            Grid baseGrid = new Grid
            {
                Background = new SolidColorBrush((Color)Application.Current.Resources["SystemChromeAltHighColor"]),
                Margin = new Thickness(0, 5, 0, 5),
                BorderThickness = new Thickness(20),
                BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemChromeAltHighColor"]),
                CornerRadius = new CornerRadius(5),
            };
            baseGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            baseGrid.ColumnDefinitions.Add(new ColumnDefinition());
            baseGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });



            // Thumbnail image
            Image thumbnailImage = new Image
            {
                Source = new BitmapImage { UriSource = Thumbnail },
                Height = 144,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            Grid.SetColumn(thumbnailImage, 0);
            baseGrid.Children.Add(thumbnailImage);



            // Data grid
            Grid dataGrid = new Grid
            {
                Margin = new Thickness(20, 0, 20, 0),
            };
            dataGrid.RowDefinitions.Add(new RowDefinition());
            dataGrid.RowDefinitions.Add(new RowDefinition());
            dataGrid.RowDefinitions.Add(new RowDefinition());
            dataGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(dataGrid, 1);
            baseGrid.Children.Add(dataGrid);

            // Title textblock
            TextBlock titleTextBlock = new TextBlock
            {
                Text = Title,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 8),
            };
            Grid.SetRow(titleTextBlock, 0);
            dataGrid.Children.Add(titleTextBlock);


            // Metadata grid
            Grid metadataGrid = new Grid();
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            metadataGrid.ColumnDefinitions.Add(new ColumnDefinition());
            metadataGrid.RowDefinitions.Add(new RowDefinition());
            metadataGrid.RowDefinitions.Add(new RowDefinition());
            metadataGrid.RowDefinitions.Add(new RowDefinition());
            metadataGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetRow(metadataGrid, 1);
            dataGrid.Children.Add(metadataGrid);
            double iconSize = 15;
            double textSize = 11;
            double iconMargin = 5;

            // Author icon
            Image authorIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Author.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(authorIcon, 0);
            Grid.SetRow(authorIcon, 0);
            metadataGrid.Children.Add(authorIcon);

            // Author data textblock
            TextBlock authorDataTextBlock = new TextBlock
            {
                Text = Author,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(authorDataTextBlock, 1);
            Grid.SetRow(authorDataTextBlock, 0);
            metadataGrid.Children.Add(authorDataTextBlock);

            // Views icon
            Image viewsIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Views.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(viewsIcon, 3);
            Grid.SetRow(viewsIcon, 0);
            metadataGrid.Children.Add(viewsIcon);

            // Views data textblock
            TextBlock viewsDataTextBlock = new TextBlock
            {
                Text = Views.ToString(),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(viewsDataTextBlock, 4);
            Grid.SetRow(viewsDataTextBlock, 0);
            metadataGrid.Children.Add(viewsDataTextBlock);

            // Date icon
            Image dateIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Date.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(dateIcon, 6);
            Grid.SetRow(dateIcon, 0);
            metadataGrid.Children.Add(dateIcon);

            // Date data textblock
            TextBlock dateDataTextBlock = new TextBlock
            {
                Text = Date.ToString((string)Config.GetValue("date_format")),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(dateDataTextBlock, 7);
            Grid.SetRow(dateDataTextBlock, 0);
            metadataGrid.Children.Add(dateDataTextBlock);

            // Duration icon
            Image durationIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Duration.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(durationIcon, 0);
            Grid.SetRow(durationIcon, 1);
            metadataGrid.Children.Add(durationIcon);

            // Duration data textblock
            TextBlock durationDataTextBlock = new TextBlock
            {
                Text = Duration.ToString(),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(durationDataTextBlock, 1);
            Grid.SetRow(durationDataTextBlock, 1);
            metadataGrid.Children.Add(durationDataTextBlock);

            // Media type & quality icon
            Image qualityIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Quality.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(qualityIcon, 3);
            Grid.SetRow(qualityIcon, 1);
            metadataGrid.Children.Add(qualityIcon);

            // Media type & quality data textblock
            string mediaTypeQualityData = MediaType == "A" ? "" : SelectedQuality;
            switch (MediaType)
            {
                case "AV": mediaTypeQualityData += $" ({ResourceLoader.GetForCurrentView().GetString("VideoPanelMediaTypeDataAV")})"; break;
                case "A": mediaTypeQualityData += $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelMediaTypeDataA")}"; break;
                case "V": mediaTypeQualityData += $" ({ResourceLoader.GetForCurrentView().GetString("VideoPanelMediaTypeDataV")})"; break;
                default: mediaTypeQualityData += $" ({ResourceLoader.GetForCurrentView().GetString("VideoPanelMediaTypeDataAV")})"; break;
            }
            TextBlock qualityDataTextBlock = new TextBlock
            {
                Text = mediaTypeQualityData,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(qualityDataTextBlock, 4);
            Grid.SetRow(qualityDataTextBlock, 1);
            metadataGrid.Children.Add(qualityDataTextBlock);

            // Trim icon
            Image trimIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Trim.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(trimIcon, 6);
            Grid.SetRow(trimIcon, 1);
            metadataGrid.Children.Add(trimIcon);

            // Trim data textblock
            TextBlock trimDataTextBlock = new TextBlock
            {
                Text = $"{TrimStart} - {TrimEnd}",
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(trimDataTextBlock, 7);
            Grid.SetRow(trimDataTextBlock, 1);
            metadataGrid.Children.Add(trimDataTextBlock);

            // Path icon
            Image pathIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Path.png") },
                Margin = new Thickness(0, iconMargin, 0, iconMargin),
                Width = iconSize,
            };
            Grid.SetColumn(pathIcon, 0);
            Grid.SetRow(pathIcon, 2);
            metadataGrid.Children.Add(pathIcon);

            // File path data textblock
            TextBlock filePathDataTextBlock = new TextBlock
            {
                Text = FilePath,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(filePathDataTextBlock, 1);
            Grid.SetColumnSpan(filePathDataTextBlock, 7);
            Grid.SetRow(filePathDataTextBlock, 2);
            metadataGrid.Children.Add(filePathDataTextBlock);


            // Progress grid
            Grid progressGrid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0),
            };
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            progressGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(progressGrid, 3);
            Grid.SetColumnSpan(progressGrid, 8);
            metadataGrid.Children.Add(progressGrid);

            // Progress icon
            ProgressIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Idle.png") },
                Width = iconSize,
            };
            Grid.SetColumn(ProgressIcon, 0);
            progressGrid.Children.Add(ProgressIcon);

            // Progress textblock
            ProgressLabelTextblock = new TextBlock
            {
                Text = ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelIdle"),
                Margin = new Thickness(10, 0, 20, 0),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize
            };
            Grid.SetColumn(ProgressLabelTextblock, 1);
            progressGrid.Children.Add(ProgressLabelTextblock);

            // Progress bar
            ProgressBar = new ProgressBar
            {
                Visibility = Visibility.Collapsed,
            };
            Grid.SetColumn(ProgressBar, 2);
            progressGrid.Children.Add(ProgressBar);

            // Buttons grid
            Grid buttonsGrid = new Grid
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            buttonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            buttonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            buttonsGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(buttonsGrid, 2);
            baseGrid.Children.Add(buttonsGrid);

            // Source icon
            AppBarButton sourceButton = new AppBarButton
            {
                Icon = new BitmapIcon { UriSource = SourceIcon, ShowAsMonochrome = false },
                Width = 40,
                Height = 48,
            };
            sourceButton.Click += SourceButtonClicked;
            Grid.SetRow(sourceButton, 0);
            buttonsGrid.Children.Add(sourceButton);

            // Delete button
            AppBarButton deleteButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Clear),
                Width = 40,
                Height = 48,
            };
            deleteButton.Click += DeleteButtonClicked;
            Grid.SetRow(deleteButton, 1);
            buttonsGrid.Children.Add(deleteButton);

            // Download/Stop button
            StartStopButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Download),
                Width = 40,
                Height = 48,
            };
            StartStopButton.Click += DownloadStopButtonClicked;
            Grid.SetRow(StartStopButton, 2);
            buttonsGrid.Children.Add(StartStopButton);

            // Return panel
            return baseGrid;
        }

        // SOURCE BUTTON
        private async void SourceButtonClicked(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(Url);
        }

        // DELETE BUTTON
        private void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            // Cancel if video downloading was started
            if (VideoStatus == VideoStatus.InProgress || VideoStatus == VideoStatus.Waiting)
                VideoTaskCancellationToken.Cancel();
            
            // Remove video from the list
            RemoveVideoFromList();
        }

        // DOWNLOAD STOP BUTTON
        private async void DownloadStopButtonClicked(object sender, RoutedEventArgs e)
        {
            // Cancel if video downloading was started
            if (VideoStatus == VideoStatus.InProgress || VideoStatus == VideoStatus.Waiting)
                VideoTaskCancellationToken.Cancel();

            // Start if video downloading wasn't started
            else
                await Start();
        }

        #endregion
    }
}
