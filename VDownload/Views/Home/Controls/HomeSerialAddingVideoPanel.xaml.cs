using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home.Controls
{
    public sealed partial class HomeSerialAddingVideoPanel : UserControl
    {
        #region CONSTANTS

        private readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public HomeSerialAddingVideoPanel(IVideo videoService)
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
        public IVideo VideoService { get; set; }

        // VIDEO DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Views { get; set; }
        private string Date { get; set; }
        private string Duration { get; set; }

        // OPTIONS CONTROL
        public HomeAddingVideoOptions HomeVideoAddingOptionsControl { get; private set; }

        #endregion



        #region EVENT HANDLERS VOIDS

        // ON CONTROL LOADING
        private async void HomeSerialAddingVideoPanel_Loading(FrameworkElement sender, object args)
        {
            await HomeVideoAddingOptionsControl.Init();
            HomeSerialAddingVideoExpander.Content = HomeVideoAddingOptionsControl;
        }

        // SOURCE BUTTON CLICKED
        private async void HomeSerialAddingVideoPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(VideoService.Url);
        }

        // DELETE BUTTON CLICKED
        private void HomeSerialAddingVideoPanelDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler DeleteRequested;

        #endregion
    }
}
