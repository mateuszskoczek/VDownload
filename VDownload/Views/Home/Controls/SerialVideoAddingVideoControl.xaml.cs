using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VDownload.Core.Extensions;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Views.Home.Controls
{
    public sealed partial class SerialVideoAddingVideoControl : UserControl
    {
        #region CONSTANTS

        private readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public SerialVideoAddingVideoControl(IVideo video)
        {
            this.InitializeComponent();

            Video = video;

            ThumbnailImage = Video.Thumbnail != null ? new BitmapImage { UriSource = Video.Thumbnail } : (BitmapImage)ImagesRes["UnknownThumbnailImage"];
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{Video.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Title = Video.Title;
            Author = Video.Author;
            Views = Video.Views.ToString();
            Date = Video.Date.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern);
            Duration = Video.Duration.ToStringOptTHBaseMMSS();

            OptionsControl = new VideoAddingOptionsControl(Video);
            Expander.Content = OptionsControl;
        }

        #endregion



        #region PROPERTIES

        public IVideo Video { get; set; }

        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Views { get; set; }
        private string Date { get; set; }
        private string Duration { get; set; }

        public VideoAddingOptionsControl OptionsControl { get; private set; }

        #endregion



        #region EVENT HANDLERS

        private async void SerialVideoAddingVideoControl_Loading(FrameworkElement sender, object args)
        {
            await OptionsControl.Init();
        }

        private async void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(Video.Url);
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion



        #region EVENTS

        public event EventHandler DeleteRequested;

        #endregion
    }
}
