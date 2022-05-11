using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Extensions;
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
    public sealed partial class VideoAddingPanel : UserControl
    {
        #region CONSTANTS

        private readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public VideoAddingPanel(IVideo video)
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
            OptionsControlPresenter.Content = OptionsControl;
        }

        #endregion



        #region PROPERTIES

        private IVideo Video { get; set; }

        // VIDEO DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Views { get; set; }
        private string Date { get; set; }
        private string Duration { get; set; }

        private VideoAddingOptionsControl OptionsControl { get; set; }

        #endregion



        #region EVENT HANDLERS

        private async void VideoAddingPanel_Loading(FrameworkElement sender, object args)
        {
            await OptionsControl.Init();
        }

        private async void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(Video.Url);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            TrimData trim = new TrimData
            {
                Start = OptionsControl.TrimStart,
                End = OptionsControl.TrimEnd,
            };
            OutputFile file;
            if (OptionsControl.FileLocation is null)
            {
                file = new OutputFile(OptionsControl.Filename, OptionsControl.FileExtension);
            }
            else
            {
                file = new OutputFile(OptionsControl.Filename, OptionsControl.FileExtension, OptionsControl.FileLocation);
            }
            string id = DownloadTasksCollectionManagement.GenerateID();
            DownloadTask downloadTask = new DownloadTask(id, Video, OptionsControl.MediaType, OptionsControl.Stream, trim, file, OptionsControl.Schedule);
            TasksAddingRequested?.Invoke(this, new DownloadTasksAddingRequestedEventArgs(new DownloadTask[] { downloadTask }, DownloadTasksAddingRequestSource.Video));
        }

        #endregion



        #region EVENTS

        public event EventHandler<DownloadTasksAddingRequestedEventArgs> TasksAddingRequested;

        #endregion
    }
}
