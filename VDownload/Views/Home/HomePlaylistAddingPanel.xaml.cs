using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Converters;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Home
{
    public sealed partial class HomePlaylistAddingPanel : UserControl
    {
        #region CONSTRUCTORS

        public HomePlaylistAddingPanel(IPlaylistService playlistService)
        {
            this.InitializeComponent();

            // Set playlist service object
            PlaylistService = playlistService;

            // Set metadata
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{PlaylistService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Name = PlaylistService.Name;

            // Add videos to list and search mins and maxes
            MinViews = PlaylistService.Videos[0].Views;
            MaxViews = PlaylistService.Videos[0].Views;
            MinDate = PlaylistService.Videos[0].Date;
            MaxDate = PlaylistService.Videos[0].Date;
            MinDuration = PlaylistService.Videos[0].Duration;
            MaxDuration = PlaylistService.Videos[0].Duration;
            foreach (IVideoService video in PlaylistService.Videos)
            {
                if (video.Views < MinViews) MinViews = video.Views;
                if (video.Views > MaxViews) MaxViews = video.Views;
                if (video.Date < MinDate) MinDate = video.Date;
                if (video.Date > MaxDate) MaxDate = video.Date;
                if (video.Duration < MinDuration) MinDuration = video.Duration;
                if (video.Duration > MaxDuration) MaxDuration = video.Duration;
                HomePlaylistAddingPanelVideoPanel videoPanel = new HomePlaylistAddingPanelVideoPanel(video);
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
                Task<StorageFolder> task = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location").AsTask();
                ATLLocation = task.Result;
                HomePlaylistAddingApplyToAllLocationSettingControl.Description = ATLLocation.Path;
            }
            else if ((bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                Task<StorageFolder> task = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("selected_media_location").AsTask();
                ATLLocation = task.Result;
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
            foreach (TimeSpan ts in new List<TimeSpan>{ MinDuration, MaxDuration } )
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
            if (Math.Floor(MaxDuration.TotalHours) > 0) HomePlaylistAddingPanelFilterMinDurationTextBox.Text += $"{Math.Floor(MinDuration.TotalHours)}:";
            if (Math.Floor(MaxDuration.TotalMinutes) > 0) HomePlaylistAddingPanelFilterMinDurationTextBox.Text += Math.Floor(MaxDuration.TotalHours) > 0 ? $"{MinDuration.Minutes:00}:" : $"{MinDuration.Minutes}:";
            HomePlaylistAddingPanelFilterMinDurationTextBox.Text += Math.Floor(MaxDuration.TotalMinutes) > 0 ? $"{MinDuration.Seconds:00}" : $"{MinDuration.Seconds}";
            if (Math.Floor(MaxDuration.TotalHours) > 0) HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += $"{Math.Floor(MaxDuration.TotalHours)}:";
            if (Math.Floor(MaxDuration.TotalMinutes) > 0) HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += Math.Floor(MaxDuration.TotalHours) > 0 ? $"{MaxDuration.Minutes:00}:" : $"{MaxDuration.Minutes}:";
            HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += Math.Floor(MaxDuration.TotalMinutes) > 0 ? $"{MaxDuration.Seconds:00}" : $"{MaxDuration.Seconds}";
        }

        #endregion



        #region PROPERTIES

        // BASE PLAYLIST DATA
        private IPlaylistService PlaylistService { get; set; }

        // PLAYLIST DATA
        private IconElement SourceImage { get; set; }
        private string Name { get; set; }

        // APPLY TO ALL OPTIONS
        public StorageFolder ATLLocation { get; set; }
        public double ATLSchedule { get; set; }

        // DELETED VIDEOS
        public List<HomePlaylistAddingPanelVideoPanel> DeletedVideos = new List<HomePlaylistAddingPanelVideoPanel>();
        public List<HomePlaylistAddingPanelVideoPanel> HiddenVideos = new List<HomePlaylistAddingPanelVideoPanel>();

        // FILTER MIN MAX
        private long MinViews { get; set; }
        private long MaxViews { get; set; }
        private DateTime MinDate { get; set; }
        private DateTime MaxDate { get; set; }
        private TimeSpan MinDuration { get; set; }
        private TimeSpan MaxDuration { get; set; }


        #endregion



        #region EVENT HANDLERS VOIDS

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

            List<HomePlaylistAddingPanelVideoPanel> allVideos = new List<HomePlaylistAddingPanelVideoPanel>();
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children) allVideos.Add(videoPanel);
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HiddenVideos) allVideos.Add(videoPanel);
            HomePlaylistAddingPanelVideosList.Children.Clear();
            HiddenVideos.Clear();

            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in allVideos)
            {
                Regex titleRegex = new Regex("");
                Regex authorRegex = new Regex("");
                try
                {
                    titleRegex = new Regex(HomePlaylistAddingPanelFilterTitleTextBox.Text);
                    authorRegex = new Regex(HomePlaylistAddingPanelFilterAuthorTextBox.Text);
                }
                catch (ArgumentException) { }
                if (!titleRegex.IsMatch(videoPanel.VideoService.Title)) HiddenVideos.Add(videoPanel);
                else if (!authorRegex.IsMatch(videoPanel.VideoService.Author)) HiddenVideos.Add(videoPanel);
                else if (HomePlaylistAddingPanelFilterMinViewsNumberBox.Value > videoPanel.VideoService.Views) HiddenVideos.Add(videoPanel);
                else if (HomePlaylistAddingPanelFilterMaxViewsNumberBox.Value < videoPanel.VideoService.Views) HiddenVideos.Add(videoPanel);
                else if (HomePlaylistAddingPanelFilterMinDateDatePicker.Date > videoPanel.VideoService.Date) HiddenVideos.Add(videoPanel);
                else if (HomePlaylistAddingPanelFilterMaxDateDatePicker.Date < videoPanel.VideoService.Date) HiddenVideos.Add(videoPanel);
                else if (minDuration > videoPanel.VideoService.Duration) HiddenVideos.Add(videoPanel);
                else if (maxDuration < videoPanel.VideoService.Duration) HiddenVideos.Add(videoPanel);
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
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children)
            {
                videoPanel.Location = ATLLocation;
                videoPanel.HomePlaylistAddingVideoPanelLocationSettingControl.Description = ATLLocation != null ? ATLLocation.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in DeletedVideos)
            {
                videoPanel.Location = ATLLocation;
                videoPanel.HomePlaylistAddingVideoPanelLocationSettingControl.Description = ATLLocation != null ? ATLLocation.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HiddenVideos)
            {
                videoPanel.Location = ATLLocation;
                videoPanel.HomePlaylistAddingVideoPanelLocationSettingControl.Description = ATLLocation != null ? ATLLocation.Path : $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }
        }

        // ATL SCHEDULE NUMBERBOX VALUE CHANGED
        private void HomePlaylistAddingApplyToAllScheduleNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            ATLSchedule = HomePlaylistAddingApplyToAllScheduleNumberBox.Value;
        }

        // APPLY ATL SCHEDULE BUTTON CLICKED
        private void HomePlaylistAddingApplyToAllApplyScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children)
            {
                videoPanel.Schedule = ATLSchedule;
                videoPanel.HomePlaylistAddingVideoPanelScheduleNumberBox.Value = ATLSchedule;
            }
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in DeletedVideos)
            {
                videoPanel.Schedule = ATLSchedule;
                videoPanel.HomePlaylistAddingVideoPanelScheduleNumberBox.Value = ATLSchedule;
            }
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HiddenVideos)
            {
                videoPanel.Schedule = ATLSchedule;
                videoPanel.HomePlaylistAddingVideoPanelScheduleNumberBox.Value = ATLSchedule;
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

                if (parsedTimeSpan < MinDuration && parsedTimeSpan > MaxDuration)
                {
                    if (Math.Floor(MaxDuration.TotalHours) > 0) HomePlaylistAddingPanelFilterMinDurationTextBox.Text += $"{Math.Floor(MinDuration.TotalHours)}:";
                    if (Math.Floor(MaxDuration.TotalMinutes) > 0) HomePlaylistAddingPanelFilterMinDurationTextBox.Text += Math.Floor(MaxDuration.TotalHours) > 0 ? $"{MinDuration.Minutes:00}:" : $"{MinDuration.Minutes}:";
                    HomePlaylistAddingPanelFilterMinDurationTextBox.Text += Math.Floor(MaxDuration.TotalMinutes) > 0 ? $"{MinDuration.Seconds:00}" : $"{MinDuration.Seconds}";
                }
            }
            else
            {
                if (Math.Floor(MaxDuration.TotalHours) > 0) HomePlaylistAddingPanelFilterMinDurationTextBox.Text += $"{Math.Floor(MinDuration.TotalHours)}:";
                if (Math.Floor(MaxDuration.TotalMinutes) > 0) HomePlaylistAddingPanelFilterMinDurationTextBox.Text += Math.Floor(MaxDuration.TotalHours) > 0 ? $"{MinDuration.Minutes:00}:" : $"{MinDuration.Minutes}:";
                HomePlaylistAddingPanelFilterMinDurationTextBox.Text += Math.Floor(MaxDuration.TotalMinutes) > 0 ? $"{MinDuration.Seconds:00}" : $"{MinDuration.Seconds}";
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

                if (parsedTimeSpan < MinDuration && parsedTimeSpan > MaxDuration)
                {
                    if (Math.Floor(MaxDuration.TotalHours) > 0) HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += $"{Math.Floor(MaxDuration.TotalHours)}:";
                    if (Math.Floor(MaxDuration.TotalMinutes) > 0) HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += Math.Floor(MaxDuration.TotalHours) > 0 ? $"{MaxDuration.Minutes:00}:" : $"{MaxDuration.Minutes}:";
                    HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += Math.Floor(MaxDuration.TotalMinutes) > 0 ? $"{MaxDuration.Seconds:00}" : $"{MaxDuration.Seconds}";
                }
            }
            else
            {
                if (Math.Floor(MaxDuration.TotalHours) > 0) HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += $"{Math.Floor(MaxDuration.TotalHours)}:";
                if (Math.Floor(MaxDuration.TotalMinutes) > 0) HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += Math.Floor(MaxDuration.TotalHours) > 0 ? $"{MaxDuration.Minutes:00}:" : $"{MaxDuration.Minutes}:";
                HomePlaylistAddingPanelFilterMaxDurationTextBox.Text += Math.Floor(MaxDuration.TotalMinutes) > 0 ? $"{MaxDuration.Seconds:00}" : $"{MaxDuration.Seconds}";
            }
            FilterChanged();
        }

        // RESTORE REMOVED VIDEOS BUTTON CLICKED
        private void HomePlaylistAddingPanelFilterRemovedRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in DeletedVideos)
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
            await Windows.System.Launcher.LaunchUriAsync(PlaylistService.PlaylistUrl);
        }

        // ADD BUTTON CLICKED
        private void HomePlaylistAddingPanelAddButton_Click(object sender, RoutedEventArgs e)
        {
            var videos = new List<(IVideoService VideoService, MediaType MediaType, IBaseStream Stream, TimeSpan TrimStart, TimeSpan TrimEnd, string Filename, MediaFileExtension Extension, StorageFolder Location, double Schedule)>();
            foreach (HomePlaylistAddingPanelVideoPanel videoPanel in HomePlaylistAddingPanelVideosList.Children)
            {
                videos.Add((videoPanel.VideoService, videoPanel.MediaType, videoPanel.Stream, videoPanel.TrimStart, videoPanel.TrimEnd, videoPanel.Filename, videoPanel.Extension, videoPanel.Location, videoPanel.Schedule));
            }
            PlaylistAddEventArgs eventArgs = new PlaylistAddEventArgs { Videos = videos.ToArray() };
            PlaylistAddRequest?.Invoke(this, eventArgs);
        }


        #endregion



        #region EVENT HANDLERS

        public event EventHandler<PlaylistAddEventArgs> PlaylistAddRequest;

        #endregion
    }
}
