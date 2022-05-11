using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Converters;
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

namespace VDownload.Views.Home
{
    public sealed partial class PlaylistAddingPanel : UserControl
    {
        #region CONSTRUCTORS

        public PlaylistAddingPanel(IPlaylist playlist)
        {
            this.InitializeComponent();

            Playlist = playlist;

            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{Playlist.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Name = Playlist.Name;

            SerialVideoAddingControl = new SerialVideoAddingControl(playlist.Videos);
            Grid.SetRow(SerialVideoAddingControl, 1);
            Base.Children.Add(SerialVideoAddingControl);
        }

        #endregion



        #region PROPERTIES

        private IPlaylist Playlist { get; set; }

        private IconElement SourceImage { get; set; }
        private string Name { get; set; }

        private SerialVideoAddingControl SerialVideoAddingControl { get; set;}

        #endregion



        #region EVENT HANDLERS

        private async void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(Playlist.Url);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            List<DownloadTask> tasksList = new List<DownloadTask>();
            foreach (SerialVideoAddingVideoControl videoControl in SerialVideoAddingControl.VideosStackPanel.Children)
            {
                TrimData trim = new TrimData
                {
                    Start = videoControl.OptionsControl.TrimStart,
                    End = videoControl.OptionsControl.TrimEnd,
                };
                OutputFile file;
                if (videoControl.OptionsControl.FileLocation is null)
                {
                    file = new OutputFile(videoControl.OptionsControl.Filename, videoControl.OptionsControl.FileExtension);
                }
                else
                {
                    file = new OutputFile(videoControl.OptionsControl.Filename, videoControl.OptionsControl.FileExtension, videoControl.OptionsControl.FileLocation);
                }
                string id = DownloadTasksCollectionManagement.GenerateID();
                tasksList.Add(new DownloadTask(id, videoControl.Video, videoControl.OptionsControl.MediaType, videoControl.OptionsControl.Stream, trim, file, videoControl.OptionsControl.Schedule));
            }

            TasksAddingRequested?.Invoke(this, new DownloadTasksAddingRequestedEventArgs(tasksList.ToArray(), DownloadTasksAddingRequestSource.Playlist));
        }

        #endregion



        #region EVENTS

        public event EventHandler<DownloadTasksAddingRequestedEventArgs> TasksAddingRequested;

        #endregion
    }
}
