using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using VDownload.Converters;
using VDownload.Core.Extensions;
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

namespace VDownload.Views.Home.Controls
{
    public sealed partial class SerialVideoAddingControl : UserControl
    {
        #region CONSTRUCTORS

        public SerialVideoAddingControl(IVideo[] videos)
        {
            this.InitializeComponent();

            Videos = videos;
            MinViews = Videos.Min(video => video.Views);
            MaxViews = Videos.Max(video => video.Views);
            MinDate = Videos.Min(video => video.Date);
            MaxDate = Videos.Max(video => video.Date);
            MinDuration = Videos.Min(video => video.Duration);
            MaxDuration = Videos.Max(video => video.Duration);

            FilterMinViewsNumberBox.Minimum = FilterMaxViewsNumberBox.Minimum = FilterMinViewsNumberBox.Value = MinViews;
            FilterMinViewsNumberBox.Maximum = FilterMaxViewsNumberBox.Maximum = FilterMaxViewsNumberBox.Value = MaxViews;

            FilterMinDateDatePicker.MinDate = FilterMaxDateDatePicker.MinDate = (DateTimeOffset)(FilterMinDateDatePicker.Date = MinDate);
            FilterMinDateDatePicker.MaxDate = FilterMaxDateDatePicker.MaxDate = (DateTimeOffset)(FilterMaxDateDatePicker.Date = MaxDate);

            TextBoxExtensions.SetMask(FilterMinDurationTextBox, (string)new TimeSpanToTextBoxMaskConverter().Convert(MaxDuration, null, null, null));
            TextBoxExtensions.SetMask(FilterMaxDurationTextBox, (string)new TimeSpanToTextBoxMaskConverter().Convert(MaxDuration, null, null, null));
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
            TextBoxExtensions.SetCustomMask(FilterMinDurationTextBox, string.Join(',', maskElementsString));
            TextBoxExtensions.SetCustomMask(FilterMaxDurationTextBox, string.Join(',', maskElementsString));
            FilterMinDurationTextBox.Text = MinDuration.ToStringOptTHMMBaseSS(MaxDuration);
            FilterMaxDurationTextBox.Text = MaxDuration.ToStringOptTHMMBaseSS();

            ApplyToAllSchedule = 0;

            foreach (IVideo video in Videos)
            {
                SerialVideoAddingVideoControl videoControl = new SerialVideoAddingVideoControl(video);
                videoControl.DeleteRequested += (s, a) =>
                {
                    DeletedVideos.Add(videoControl);
                    FilterRemovedTextBlock.Visibility = Visibility.Visible;
                    FilterRemovedCountTextBlock.Visibility = Visibility.Visible;
                    FilterRemovedRestoreButton.Visibility = Visibility.Visible;
                    FilterRemovedCountTextBlock.Text = $"{DeletedVideos.Count}";
                    FilterHeaderCountTextBlock.Text = HiddenVideos.Count + DeletedVideos.Count > 0 ? $"{ResourceLoader.GetForCurrentView().GetString("Home_Adding_Base_SerialVideoAddingControl_Filter_Header_CountTextBlockPrefix")}: {HiddenVideos.Count + DeletedVideos.Count}" : "";
                    VideosStackPanel.Children.Remove(videoControl);
                };
                VideosStackPanel.Children.Add(videoControl);
            }
        }

        #endregion



        #region PROPERTIES

        private IVideo[] Videos { get; set; }

        public StorageFolder ApplyToAllLocation { get; set; }
        public double ApplyToAllSchedule { get; set; }

        private List<SerialVideoAddingVideoControl> DeletedVideos { get; set; } = new List<SerialVideoAddingVideoControl>();
        private List<SerialVideoAddingVideoControl> HiddenVideos { get; set; } = new List<SerialVideoAddingVideoControl>();

        private long MinViews { get; set; }
        private long MaxViews { get; set; }
        private DateTime MinDate { get; set; }
        private DateTime MaxDate { get; set; }
        private TimeSpan MinDuration { get; set; }
        private TimeSpan MaxDuration { get; set; }

        #endregion



        #region EVENT HANDLERS

        private async void SerialVideoAddingControl_Loading(FrameworkElement sender, object args)
        {
            if (!(bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("last_media_location"))
            {
                ApplyToAllLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location");
                ApplyToAllLocationSettingControl.Description = ApplyToAllLocation.Path;
            }
            else if ((bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                ApplyToAllLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("selected_media_location");
                ApplyToAllLocationSettingControl.Description = ApplyToAllLocation.Path;
            }
            else
            {
                ApplyToAllLocation = null;
                ApplyToAllLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }
        }

        private async void ApplyToAllLocationBrowseButton_Click(object sender, RoutedEventArgs e)
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
                    await (await selectedFolder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("last_media_location", selectedFolder);
                    ApplyToAllLocation = selectedFolder;
                    ApplyToAllLocationSettingControl.Description = ApplyToAllLocation.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        private void ApplyToAllApplyLocationButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SerialVideoAddingVideoControl videoPanel in VideosStackPanel.Children)
            {
                videoPanel.OptionsControl.ChangeLocation(ApplyToAllLocation);
            }
            foreach (SerialVideoAddingVideoControl videoPanel in DeletedVideos)
            {
                videoPanel.OptionsControl.ChangeLocation(ApplyToAllLocation);
            }
            foreach (SerialVideoAddingVideoControl videoPanel in HiddenVideos)
            {
                videoPanel.OptionsControl.ChangeLocation(ApplyToAllLocation);
            }
        }

        private void ApplyToAllScheduleNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(ApplyToAllScheduleNumberBox.Value))
            {
                ApplyToAllScheduleNumberBox.Value = ApplyToAllSchedule;
            }
            else
            {
                ApplyToAllSchedule = ApplyToAllScheduleNumberBox.Value;
            }
        }

        private void ApplyToAllApplyScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SerialVideoAddingVideoControl videoPanel in VideosStackPanel.Children)
            {
                videoPanel.OptionsControl.ChangeSchedule(ApplyToAllSchedule);
            }
            foreach (SerialVideoAddingVideoControl videoPanel in DeletedVideos)
            {
                videoPanel.OptionsControl.ChangeSchedule(ApplyToAllSchedule);
            }
            foreach (SerialVideoAddingVideoControl videoPanel in HiddenVideos)
            {
                videoPanel.OptionsControl.ChangeSchedule(ApplyToAllSchedule);
            }
        }

        private void FilterTitleAndAuthorTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            FilterChanged();
        }

        private void FilterMinViewsNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(FilterMinViewsNumberBox.Value))
            {
                FilterMinViewsNumberBox.Value = MinViews;
            }
            FilterChanged();
        }

        private void FilterMaxViewsNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (double.IsNaN(FilterMaxViewsNumberBox.Value))
            {
                FilterMaxViewsNumberBox.Value = MaxViews;
            }
            FilterChanged();
        }

        private void FilterMinAndMaxDateDatePicker_LostFocus(object sender, RoutedEventArgs args)
        {
            FilterChanged();
        }

        private void FilterMinDurationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!FilterMinDurationTextBox.Text.Contains('_'))
            {
                string[] segments = FilterMinDurationTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < MinDuration || parsedTimeSpan > MaxDuration)
                {
                    FilterMinDurationTextBox.Text = MinDuration.ToStringOptTHMMBaseSS(MaxDuration);
                }
            }
            else
            {
                FilterMinDurationTextBox.Text = MinDuration.ToStringOptTHMMBaseSS(MaxDuration);
            }
            FilterChanged();
        }

        private void FilterMaxDurationTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!FilterMaxDurationTextBox.Text.Contains('_'))
            {
                string[] segments = FilterMaxDurationTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < MinDuration || parsedTimeSpan > MaxDuration)
                {
                    FilterMaxDurationTextBox.Text = MaxDuration.ToStringOptTHMMBaseSS();
                }
            }
            else
            {
                FilterMaxDurationTextBox.Text = MaxDuration.ToStringOptTHMMBaseSS();
            }
            FilterChanged();
        }

        private void FilterRemovedRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SerialVideoAddingVideoControl videoControl in DeletedVideos)
            {
                VideosStackPanel.Children.Add(videoControl);
            }
            FilterRemovedTextBlock.Visibility = Visibility.Collapsed;
            FilterRemovedCountTextBlock.Visibility = Visibility.Collapsed;
            FilterRemovedRestoreButton.Visibility = Visibility.Collapsed;
            FilterHeaderCountTextBlock.Text = HiddenVideos.Count + DeletedVideos.Count > 0 ? $"{ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterHeaderCountTextBlockPrefix")}: {HiddenVideos.Count + DeletedVideos.Count}" : "";
            DeletedVideos.Clear();
        }

        #endregion



        #region PRIVATE METHODS

        private void FilterChanged()
        {
            string[] minSegments = FilterMinDurationTextBox.Text.Split(':').Reverse().ToArray();
            int.TryParse(minSegments.ElementAtOrDefault(0), out int minSeconds);
            int.TryParse(minSegments.ElementAtOrDefault(1), out int minMinutes);
            int.TryParse(minSegments.ElementAtOrDefault(2), out int minHours);
            TimeSpan minDuration = new TimeSpan(minHours, minMinutes, minSeconds);

            string[] maxSegments = FilterMaxDurationTextBox.Text.Split(':').Reverse().ToArray();
            int.TryParse(maxSegments.ElementAtOrDefault(0), out int maxSeconds);
            int.TryParse(maxSegments.ElementAtOrDefault(1), out int maxMinutes);
            int.TryParse(maxSegments.ElementAtOrDefault(2), out int maxHours);
            TimeSpan maxDuration = new TimeSpan(maxHours, maxMinutes, maxSeconds);

            Regex titleRegex = new Regex("");
            Regex authorRegex = new Regex("");
            try
            {
                titleRegex = new Regex(FilterTitleTextBox.Text);
                authorRegex = new Regex(FilterAuthorTextBox.Text);
            }
            catch (ArgumentException) { }

            List<SerialVideoAddingVideoControl> allVideos = new List<SerialVideoAddingVideoControl>();
            allVideos.AddRange((IEnumerable<SerialVideoAddingVideoControl>)(VideosStackPanel.Children.ToList()));
            allVideos.AddRange(HiddenVideos);
            VideosStackPanel.Children.Clear();
            HiddenVideos.Clear();
            foreach (SerialVideoAddingVideoControl videoControl in allVideos)
            {
                if (
                    !titleRegex.IsMatch(videoControl.Video.Title) ||
                    !authorRegex.IsMatch(videoControl.Video.Author) ||
                    FilterMinViewsNumberBox.Value > videoControl.Video.Views ||
                    FilterMaxViewsNumberBox.Value < videoControl.Video.Views ||
                    FilterMinDateDatePicker.Date > videoControl.Video.Date ||
                    FilterMaxDateDatePicker.Date < videoControl.Video.Date ||
                    minDuration > videoControl.Video.Duration ||
                    maxDuration < videoControl.Video.Duration
                ) HiddenVideos.Add(videoControl);
                else VideosStackPanel.Children.Add(videoControl);
            }
            FilterHeaderCountTextBlock.Text = HiddenVideos.Count + DeletedVideos.Count > 0 ? $"{ResourceLoader.GetForCurrentView().GetString("HomePlaylistAddingPanelFilterHeaderCountTextBlockPrefix")}: {HiddenVideos.Count + DeletedVideos.Count}" : string.Empty;
        }

        #endregion
    }
}
