using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using VDownload.Core.Structs;
using VDownload.Views.Home.Controls;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home
{
    public sealed partial class HomeVideoAddingPanel : UserControl
    {
        #region CONSTANTS

        private readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public HomeVideoAddingPanel(IVideo videoService)
        {
            this.InitializeComponent();

            // Set video service object
            VideoService = videoService;

            // Set metadata
            ThumbnailImage = VideoService.Metadata.Thumbnail != null ? new BitmapImage { UriSource = VideoService.Metadata.Thumbnail } : (BitmapImage)ImagesRes["UnknownThumbnailImage"];
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{VideoService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Title = VideoService.Metadata.Title;
            Author = VideoService.Metadata.Author;
            Views = VideoService.Metadata.Views.ToString();
            Date = VideoService.Metadata.Date.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern);
            Duration = TimeSpanCustomFormat.ToOptTHBaseMMSS(VideoService.Metadata.Duration);

            // Set video options control
            HomeVideoAddingOptionsControl = new HomeAddingVideoOptions(VideoService);
        }

        #endregion



        #region PROPERTIES

        // BASE VIDEO DATA
        private IVideo VideoService { get; set; }

        // VIDEO DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Views { get; set; }
        private string Date { get; set; }
        private string Duration { get; set; }

        // OPTIONS CONTROL
        private HomeAddingVideoOptions HomeVideoAddingOptionsControl { get; set; }

        #endregion



        #region EVENT HANDLERS VOIDS

        private async void HomeVideoAddingPanel_Loading(FrameworkElement sender, object args)
        {
            await HomeVideoAddingOptionsControl.Init();
            HomeVideoAddingOptionsControlParent.Content = HomeVideoAddingOptionsControl;
        }

        // SOURCE BUTTON CLICKED
        public async void HomeVideoAddingPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(VideoService.Url);
        }

        // ADD BUTTON CLICKED
        public void HomeVideoAddingPanelAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Pack task data
            TaskData taskData = new TaskData
            {
                VideoService = VideoService,
                TaskOptions = new TaskOptions()
                {
                    MediaType = HomeVideoAddingOptionsControl.MediaType,
                    Stream = HomeVideoAddingOptionsControl.Stream,
                    TrimStart = HomeVideoAddingOptionsControl.TrimStart,
                    TrimEnd = HomeVideoAddingOptionsControl.TrimEnd,
                    Filename = HomeVideoAddingOptionsControl.Filename,
                    Extension = HomeVideoAddingOptionsControl.Extension,
                    Location = HomeVideoAddingOptionsControl.Location,
                    Schedule = HomeVideoAddingOptionsControl.Schedule,
                }
            };

            // Request task adding
            TasksAddingRequestedEventArgs eventArgs = new TasksAddingRequestedEventArgs
            {
                TaskData = new TaskData[] { taskData },
                RequestSource = TaskAddingRequestSource.Video
            };
            TasksAddingRequested?.Invoke(this, eventArgs);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<TasksAddingRequestedEventArgs> TasksAddingRequested;

        #endregion
    }
}
