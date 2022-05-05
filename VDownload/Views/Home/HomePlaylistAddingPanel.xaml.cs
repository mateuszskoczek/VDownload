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
    public sealed partial class HomePlaylistAddingPanel : UserControl
    {
        #region CONSTRUCTORS

        public HomePlaylistAddingPanel(IPlaylist playlistService)
        {
            this.InitializeComponent();

            // Set playlist service object
            PlaylistService = playlistService;
        }

        #endregion



        #region PROPERTIES

        // BASE PLAYLIST DATA
        private IPlaylist PlaylistService { get; set; }

        // PLAYLIST DATA
        private IconElement SourceImage { get; set; }
        private string Name { get; set; }

        // APPLY TO ALL OPTIONS
        public StorageFolder ATLLocation { get; set; }
        public double ATLSchedule { get; set; }

        // DELETED VIDEOS
        public List<HomeSerialAddingVideoPanel> DeletedVideos = new List<HomeSerialAddingVideoPanel>();
        public List<HomeSerialAddingVideoPanel> HiddenVideos = new List<HomeSerialAddingVideoPanel>();

        // FILTER MIN MAX
        private long MinViews { get; set; }
        private long MaxViews { get; set; }
        private DateTime MinDate { get; set; }
        private DateTime MaxDate { get; set; }
        private TimeSpan MinDuration { get; set; }
        private TimeSpan MaxDuration { get; set; }


        #endregion



        #region EVENT HANDLERS VOIDS

        // ON CONTROL LOADING
        private async void HomePlaylistAddingPanel_Loading(FrameworkElement sender, object args)
        {
            // Set metadata
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{PlaylistService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Name = PlaylistService.Name;

            // Add videos to list and search mins and maxes
            MinViews = PlaylistService.Videos[0].Metadata.Views;
            MaxViews = PlaylistService.Videos[0].Metadata.Views;
            MinDate = PlaylistService.Videos[0].Metadata.Date;
            MaxDate = PlaylistService.Videos[0].Metadata.Date;
            MinDuration = PlaylistService.Videos[0].Metadata.Duration;
            MaxDuration = PlaylistService.Videos[0].Metadata.Duration;
            foreach (IVideo video in PlaylistService.Videos)
            {
                // Set mins and maxes
                if (video.Metadata.Views < MinViews) MinViews = video.Metadata.Views;
                if (video.Metadata.Views > MaxViews) MaxViews = video.Metadata.Views;
                if (video.Metadata.Date < MinDate) MinDate = video.Metadata.Date;
                if (video.Metadata.Date > MaxDate) MaxDate = video.Metadata.Date;
                if (video.Metadata.Duration < MinDuration) MinDuration = video.Metadata.Duration;
                if (video.Metadata.Duration > MaxDuration) MaxDuration = video.Metadata.Duration;

                // Add videos to list
                HomeSerialAddingVideoPanel videoPanel = new HomeSerialAddingVideoPanel(video);
                videoPanel.DeleteRequested += (s, a) =>
                {
                    DeletedVideos.Add(videoPanel);
                    HomePlaylistAddingPanelFilterRemovedTextBlock.Visibility = Visibility.Visible;
                    HomePlaylistAddingPanelFilterRemovedCountTextBlock.Visibility = Visibility.Visible;
                    HomePlaylistAddingPanelFilterRemovedRestoreButton.Visibility = Visibility.Visible;
                    HomePlaylistAddingPanelFilterRemovedCountTextBlock.Text = $"{DeletedVideos.Count}";
                    HomePlaylistAddingPanelFilterHeaderCountTextBlock.Text = HiddenVideos.Count + DeletedVideos.Count > 0 ? $"{ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterHeaderCountTextBlockPrefix")}: {HiddenVideos.Count + DeletedVideos.Count}" : "";
                    HomePlaylistAddingPanelVideosList.Children.Remove(videoPanel);
                };
                HomePlaylistAddingPanelVideosList.Children.Add(videoPanel);
            }

            // Set apply to all location option
            if (!(bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("last_media_location"))
            {
                ATLLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location");
                HomePlaylistAddingApplyToAllLocationSettingControl.Description = ATLLocation.Path;
            }
            else if ((bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                ATLLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("selected_media_location");
                HomePlaylistAddingApplyToAllLocationSettingControl.Description = ATLLocation.Path;
            }
            else
            {
                ATLLocation = null;
                HomePlaylistAddingApplyToAllLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }

            // Set apply to all schedule option
            ATLSchedule = 0;

            // Set title and author filter option
            HomePlaylistAddingPanelFilterTitleTextBox.PlaceholderText = ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterTitleTextBoxPlaceholderText");
            HomePlaylistAddingPanelFilterAuthorTextBox.PlaceholderText = ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterAuthorTextBoxPlaceholderText");

            // Set views filter option
            HomePlaylistAddingPanelFilterMinViewsNumberBox.Minimum = HomePlaylistAddingPanelFilterMaxViewsNumberBox.Minimum = HomePlaylistAddingPanelFilterMinViewsNumberBox.Value = MinViews;
            HomePlaylistAddingPanelFilterMinViewsNumberBox.Maximum = HomePlaylistAddingPanelFilterMaxViewsNumberBox.Maximum = HomePlaylistAddingPanelFilterMaxViewsNumberBox.Value = MaxViews;

            // Set date filter option
            HomePlaylistAddingPanelFilterMinDateDatePicker.MinDate = HomePlaylistAddingPanelFilterMaxDateDatePicker.MinDate = (DateTimeOffset)(HomePlaylistAddingPanelFilterMinDateDatePicker.Date = MinDate);
            HomePlaylistAddingPanelFilterMinDateDatePicker.MaxDate = HomePlaylistAddingPanelFilterMaxDateDatePicker.MaxDate = (DateTimeOffset)(HomePlaylistAddingPanelFilterMaxDateDatePicker.Date = MaxDate);

            // Set duration filter option
            TextBoxExtensions.SetMask(HomePlaylistAddingPanelFilterMinDurationTextBox, (string)new TimeSpanToTextBoxMaskConverter().Convert(MaxDuration, null, null, null));
            TextBoxExtensions.SetMask(HomePlaylistAddingPanelFilterMaxDurationTextBox, (string)new TimeSpanToTextBoxMaskConverter().Convert(MaxDuration, null, null, null));
            HashSet<int> maskElements = new HashSet<int>();
            foreach (TimeSpan ts in new List<TimeSpan> { MinDuration, MaxDuration })
            {
                if (Math.Floor(ts.TotalHours) > 0) maskElements.Add(int.Parse(Math.Floor(ts.TotalHours).ToString()[0].ToString()));
                if (Math.Floor(ts.TotalMinutes) > 0)
                {
                    if (Math.Floor(ts.TotalHours) > 0) maskElements.Add(5);
                    else maskElements.Add(int.Parse(ts.Minutes.ToString()[0].ToString()));
                }
                if (Math.Floor(ts.TotalMinutes) > 0) maskElements.Add(5);
                else maskElements.Add(int.Parse(ts.Seconds.ToString()[0].ToString()));
            }
            List<string> maskElementsString = new List<string>();
            foreach (int i in maskElements)
            {
                if (i != 9) maskElementsString.Add($"{i}:[0-{i}]");
            }
            TextBoxExtensions.SetCustomMask(HomePlaylistAddingPanelFilterMinDurationTextBox, string.Join(',', maskElementsString));
            TextBoxExtensions.SetCustomMask(HomePlaylistAddingPanelFilterMaxDurationTextBox, string.Join(',', maskElementsString));
            HomePlaylistAddingPanelFilterMinDurationTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(MinDuration, MaxDuration);
            HomePlaylistAddingPanelFilterMaxDurationTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(MaxDuration);
        }

        // FILTER CHANGED
        private void FilterChanged()
        {
            // Min duration
            string[] minSegments = HomePlaylistAddingPanelFilterMinDurationTextBox.Text.Split(':').Reverse().ToArray();
            int.TryParse(minSegments.ElementAtOrDefault(0), out int minSeconds);
            int.TryParse(minSegments.ElementAtOrDefault(1), out int minMinutes);
            int.TryParse(minSegments.ElementAtOrDefault(2), out int minHours);
            TimeSpan minDuration = new TimeSpan(minHours, minMinutes, minSeconds);

            // Max duration
            string[] maxSegments = HomePlaylistAddingPanelFilterMaxDurationTextBox.Text.Split(':').Reverse().ToArray();
            int.TryParse(maxSegments.ElementAtOrDefault(0), out int maxSeconds);
            int.TryParse(maxSegments.ElementAtOrDefault(1), out int maxMinutes);
            int.TryParse(maxSegments.ElementAtOrDefault(2), out int maxHours);
            TimeSpan maxDuration = new TimeSpan(maxHours, maxMinutes, maxSeconds);

            // Title and author regex
            Regex titleRegex = new Regex("");
            Regex authorRegex = new Regex("");
            try
            {
                titleRegex = new Regex(HomePlaylistAddingPanelFilterTitleTextBox.Text);
                authorRegex = new Regex(HomePlaylistAddingPanelFilterAuthorTextBox.Text);
            }
            catch (ArgumentException) { }

            List<HomeSerialAddingVideoPanel> allVideos = new List<HomeSerialAddingVideoPanel>();
            foreach (HomeSerialAddingVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children) allVideos.Add(videoPanel);
            foreach (HomeSerialAddingVideoPanel videoPanel in HiddenVideos) allVideos.Add(videoPanel);
            HomePlaylistAddingPanelVideosList.Children.Clear();
            HiddenVideos.Clear();

            foreach (HomeSerialAddingVideoPanel videoPanel in allVideos)
            {
                if (
                    !titleRegex.IsMatch(videoPanel.VideoService.Metadata.Title) ||
                    !authorRegex.IsMatch(videoPanel.VideoService.Metadata.Author) ||
                    HomePlaylistAddingPanelFilterMinViewsNumberBox.Value > videoPanel.VideoService.Metadata.Views ||
                    HomePlaylistAddingPanelFilterMaxViewsNumberBox.Value < videoPanel.VideoService.Metadata.Views ||
                    HomePlaylistAddingPanelFilterMinDateDatePicker.Date > videoPanel.VideoService.Metadata.Date ||
                    HomePlaylistAddingPanelFilterMaxDateDatePicker.Date < videoPanel.VideoService.Metadata.Date ||
                    minDuration > videoPanel.VideoService.Metadata.Duration ||
                    maxDuration < videoPanel.VideoService.Metadata.Duration
                ) HiddenVideos.Add(videoPanel);
                else HomePlaylistAddingPanelVideosList.Children.Add(videoPanel);
            }

            HomePlaylistAddingPanelFilterHeaderCountTextBlock.Text = HiddenVideos.Count + DeletedVideos.Count > 0 ? $"{ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterHeaderCountTextBlockPrefix")}: {HiddenVideos.Count + DeletedVideos.Count}" : "";
        }

        // ATL LOCATION BROWSE BUTTON CLICKED
        private async void HomePlaylistAddingApplyToAllLocationBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");

            StorageFolder selectedFolder = await picker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                try
                {
                    await(await selectedFolder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("last_media_location", selectedFolder);
                    ATLLocation = selectedFolder;
                    HomePlaylistAddingApplyToAllLocationSettingControl.Description = ATLLocation.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        // APPLY ATL LOCATION BUTTON CLICKED
        private void HomePlaylistAddingApplyToAllApplyLocationButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HomeSerialAddingVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children)
            {
                videoPanel.HomeVideoAddingOptionsControl.ChangeLocation(ATLLocation);
            }
            foreach (HomeSerialAddingVideoPanel videoPanel in DeletedVideos)
            {
                videoPanel.HomeVideoAddingOptionsControl.ChangeLocation(ATLLocation);
            }
            foreach (HomeSerialAddingVideoPanel videoPanel in HiddenVideos)
            {
                videoPanel.HomeVideoAddingOptionsControl.ChangeLocation(ATLLocation);
            }
        }

        // ATL SCHEDULE NUMBERBOX VALUE CHANGED
        private void HomePlaylistAddingApplyToAllScheduleNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(HomePlaylistAddingApplyToAllScheduleNumberBox.Value)) HomePlaylistAddingApplyToAllScheduleNumberBox.Value = ATLSchedule;
            else ATLSchedule = HomePlaylistAddingApplyToAllScheduleNumberBox.Value;
        }

        // APPLY ATL SCHEDULE BUTTON CLICKED
        private void HomePlaylistAddingApplyToAllApplyScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HomeSerialAddingVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children)
            {
                videoPanel.HomeVideoAddingOptionsControl.ChangeSchedule(ATLSchedule);
            }
            foreach (HomeSerialAddingVideoPanel videoPanel in DeletedVideos)
            {
                videoPanel.HomeVideoAddingOptionsControl.ChangeSchedule(ATLSchedule);
            }
            foreach (HomeSerialAddingVideoPanel videoPanel in HiddenVideos)
            {
                videoPanel.HomeVideoAddingOptionsControl.ChangeSchedule(ATLSchedule);
            }
        }

        // TITLE AND AUTHOR FILTERS TEXTBOXS TEXT CHANGED
        private void HomePlaylistAddingPanelFilterTitleAndAuthorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            FilterChanged();
        }

        // MIN VIEWS FILTERS NUMBERBOX VALUE CHANGED
        private void HomePlaylistAddingPanelFilterMinViewsNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(HomePlaylistAddingPanelFilterMinViewsNumberBox.Value)) HomePlaylistAddingPanelFilterMinViewsNumberBox.Value = MinViews;
            FilterChanged();
        }

        // MAX VIEWS FILTERS NUMBERBOX VALUE CHANGED
        private void HomePlaylistAddingPanelFilterMaxViewsNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(HomePlaylistAddingPanelFilterMaxViewsNumberBox.Value)) HomePlaylistAddingPanelFilterMaxViewsNumberBox.Value = MaxViews;
            FilterChanged();
        }

        // MIN AND MAX DATE FILTERS DATEPICKER VALUE CHANGED
        private void HomePlaylistAddingPanelFilterMinAndMaxDateDatePicker_LostFocus(object sender, RoutedEventArgs args)
        {
            FilterChanged();
        }

        // MIN DURATION FILTERS TEXTBOX VALUE CHANGED
        private void HomePlaylistAddingPanelFilterMinDurationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!HomePlaylistAddingPanelFilterMinDurationTextBox.Text.Contains('_'))
            {
                string[] segments = HomePlaylistAddingPanelFilterMinDurationTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < MinDuration || parsedTimeSpan > MaxDuration)
                {
                    HomePlaylistAddingPanelFilterMinDurationTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(MinDuration, MaxDuration);
                }
            }
            else
            {
                HomePlaylistAddingPanelFilterMinDurationTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(MinDuration, MaxDuration);
            }
            FilterChanged();
        }

        // MAX DURATION FILTERS TEXTBOX VALUE CHANGED
        private void HomePlaylistAddingPanelFilterMaxDurationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!HomePlaylistAddingPanelFilterMaxDurationTextBox.Text.Contains('_'))
            {
                string[] segments = HomePlaylistAddingPanelFilterMaxDurationTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < MinDuration || parsedTimeSpan > MaxDuration)
                {
                    HomePlaylistAddingPanelFilterMaxDurationTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(MaxDuration);
                }
            }
            else
            {
                HomePlaylistAddingPanelFilterMaxDurationTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(MaxDuration);
            }
            FilterChanged();
        }

        // RESTORE REMOVED VIDEOS BUTTON CLICKED
        private void HomePlaylistAddingPanelFilterRemovedRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HomeSerialAddingVideoPanel videoPanel in DeletedVideos)
            {
                HomePlaylistAddingPanelVideosList.Children.Add(videoPanel);
            }
            HomePlaylistAddingPanelFilterRemovedTextBlock.Visibility = Visibility.Collapsed;
            HomePlaylistAddingPanelFilterRemovedCountTextBlock.Visibility = Visibility.Collapsed;
            HomePlaylistAddingPanelFilterRemovedRestoreButton.Visibility = Visibility.Collapsed;
            HomePlaylistAddingPanelFilterHeaderCountTextBlock.Text = HiddenVideos.Count + DeletedVideos.Count > 0 ? $"{ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterHeaderCountTextBlockPrefix")}: {HiddenVideos.Count + DeletedVideos.Count}" : "";
            DeletedVideos.Clear();
        }

        // SOURCE BUTTON CLICKED
        private async void HomePlaylistAddingPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(PlaylistService.Url);
        }

        // ADD BUTTON CLICKED
        private void HomePlaylistAddingPanelAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Pack tasks data
            List<TaskData> taskDataList = new List<TaskData>();
            foreach (HomeSerialAddingVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children)
            {
                TaskData taskData = new TaskData
                {
                    VideoService = videoPanel.VideoService,
                    TaskOptions = new TaskOptions()
                    {
                        MediaType = videoPanel.HomeVideoAddingOptionsControl.MediaType,
                        Stream = videoPanel.HomeVideoAddingOptionsControl.Stream,
                        TrimStart = videoPanel.HomeVideoAddingOptionsControl.TrimStart,
                        TrimEnd = videoPanel.HomeVideoAddingOptionsControl.TrimEnd,
                        Filename = videoPanel.HomeVideoAddingOptionsControl.Filename,
                        Extension = videoPanel.HomeVideoAddingOptionsControl.Extension,
                        Location = videoPanel.HomeVideoAddingOptionsControl.Location,
                        Schedule = videoPanel.HomeVideoAddingOptionsControl.Schedule,
                    }
                };
                taskDataList.Add(taskData);
            }

            // Request tasks adding
            TasksAddingRequestedEventArgs eventArgs = new TasksAddingRequestedEventArgs 
            { 
                TaskData = taskDataList.ToArray(),
                RequestSource = TaskAddingRequestSource.Playlist
            };
            TasksAddingRequested?.Invoke(this, eventArgs);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<TasksAddingRequestedEventArgs> TasksAddingRequested;

        #endregion
    }
}
