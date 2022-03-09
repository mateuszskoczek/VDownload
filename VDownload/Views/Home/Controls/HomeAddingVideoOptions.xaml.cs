using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using VDownload.Core.Structs;
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
    public sealed partial class HomeAddingVideoOptions : UserControl
    {
        #region CONSTRUCTORS

        public HomeAddingVideoOptions(IVideoService videoService)
        {
            this.InitializeComponent();

            // Set video service
            VideoService = videoService;
        }

        // INIT CONTROL
        public async Task Init()
        {
            // Set media type
            foreach (string mediaType in Enum.GetNames(typeof(MediaType)))
            {
                HomeAddingMediaTypeSettingControlComboBox.Items.Add(ResourceLoader.GetForCurrentView().GetString($"MediaType{mediaType}Text"));
            }
            HomeAddingMediaTypeSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_media_type");

            // Set quality
            foreach (BaseStream stream in VideoService.BaseStreams)
            {
                HomeAddingQualitySettingControlComboBox.Items.Add($"{stream.Height}p{(stream.FrameRate > 0 ? stream.FrameRate.ToString() : "N/A")}");
            }
            HomeAddingQualitySettingControlComboBox.SelectedIndex = 0;

            // Set trim start
            TrimStart = TimeSpan.Zero;
            HomeAddingTrimStartTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(TrimStart, VideoService.Metadata.Duration);

            // Set trim end
            TrimEnd = VideoService.Metadata.Duration;
            HomeAddingTrimEndTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(TrimEnd);

            // Set filename
            string temporaryFilename = (string)Config.GetValue("default_filename");
            Dictionary<string, string> filenameStandardTemplates = new Dictionary<string, string>()
            {
                { "<title>", VideoService.Metadata.Title },
                { "<author>", VideoService.Metadata.Author },
                { "<views>", VideoService.Metadata.Views.ToString() },
                { "<id>", VideoService.ID },
            };
            foreach (KeyValuePair<string, string> template in filenameStandardTemplates) temporaryFilename = temporaryFilename.Replace(template.Key, template.Value);
            Dictionary<Regex, IFormattable> filenameFormatTemplates = new Dictionary<Regex, IFormattable>()
            {
                { new Regex(@"<date_pub:(?<format>.*)>"), VideoService.Metadata.Date },
                { new Regex(@"<date_now:(?<format>.*)>"), DateTime.Now },
                { new Regex(@"<duration:(?<format>.*)>"), VideoService.Metadata.Duration },
            };
            foreach (KeyValuePair<Regex, IFormattable> template in filenameFormatTemplates) foreach (Match templateMatch in template.Key.Matches(temporaryFilename)) temporaryFilename = temporaryFilename.Replace(templateMatch.Value, template.Value.ToString(templateMatch.Groups["format"].Value, null));
            foreach (char c in Path.GetInvalidFileNameChars()) temporaryFilename = temporaryFilename.Replace(c, ' ');
            Filename = temporaryFilename;
            HomeAddingFilenameTextBox.Text = Filename;

            // Set location
            if (!(bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("last_media_location"))
            {
                Location = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location");
                HomeAddingLocationSettingControl.Description = Location.Path;
            }
            else if ((bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                Location = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("selected_media_location");
                HomeAddingLocationSettingControl.Description = Location.Path;
            }
            else
            {
                Location = null;
                HomeAddingLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }

            // Set schedule
            Schedule = 0;
        }

        #endregion



        #region PROPERTIES

        // VIDEO SERVICE
        private IVideoService VideoService { get; set; }

        // TASK OPTIONS
        public MediaType MediaType { get; private set; }
        public BaseStream Stream { get; private set; }
        public TimeSpan TrimStart { get; private set; }
        public TimeSpan TrimEnd { get; private set; }
        public string Filename { get; private set; }
        public MediaFileExtension Extension { get; private set; }
        public StorageFolder Location { get; private set; }
        public double Schedule { get; private set; }

        #endregion



        #region CHANGE PROPERTIES FROM PARENT VOIDS

        // LOCATION
        public void ChangeLocation(StorageFolder location)
        {
            Location = location;
            HomeAddingLocationSettingControl.Description = Location.Path ?? $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
        }

        // SCHEDULE
        public void ChangeSchedule(double schedule)
        {
            Schedule = schedule;
            HomeAddingScheduleNumberBox.Value = Schedule;
        }

        #endregion



        #region EVENT HANDLERS VOIDS

        // MEDIA TYPE COMBOBOX SELECTION CHANGED
        private void HomeAddingMediaTypeSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaType = (MediaType)HomeAddingMediaTypeSettingControlComboBox.SelectedIndex;
            if (HomeAddingMediaTypeSettingControlComboBox.SelectedIndex == (int)MediaType.OnlyAudio)
            {
                HomeAddingQualitySettingControl.Visibility = Visibility.Collapsed;
                HomeAddingQualitySettingControlComboBox.SelectedIndex = VideoService.BaseStreams.Count() - 1;

                HomeAddingExtensionComboBox.Items.Clear();
                foreach (AudioFileExtension extension in Enum.GetValues(typeof(AudioFileExtension)))
                {
                    HomeAddingExtensionComboBox.Items.Add(extension);
                }
                HomeAddingExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_audio_extension") - 3;
            }
            else
            {
                HomeAddingQualitySettingControl.Visibility = Visibility.Visible;
                HomeAddingQualitySettingControlComboBox.SelectedIndex = 0;

                HomeAddingExtensionComboBox.Items.Clear();
                foreach (VideoFileExtension extension in Enum.GetValues(typeof(VideoFileExtension)))
                {
                    HomeAddingExtensionComboBox.Items.Add(extension);
                }
                HomeAddingExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_video_extension");
            }
        }

        // QUALITY COMBOBOX SELECTION CHANGED
        private void HomeAddingQualitySettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stream = VideoService.BaseStreams[HomeAddingQualitySettingControlComboBox.SelectedIndex];
        }

        // TRIM START TEXTBOX LOST FOCUS
        private void HomeAddingTrimStartTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!HomeAddingTrimStartTextBox.Text.Contains('_'))
            {
                string[] segments = HomeAddingTrimStartTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < VideoService.Metadata.Duration && parsedTimeSpan > TimeSpan.Zero) TrimStart = parsedTimeSpan;
                else
                {
                    TrimStart = TimeSpan.Zero;
                    HomeAddingTrimStartTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(TrimStart, VideoService.Metadata.Duration);
                }
            }
        }

        // TRIM END TEXTBOX LOST FOCUS
        private void HomeAddingTrimEndTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!HomeAddingTrimEndTextBox.Text.Contains('_'))
            {
                string[] segments = HomeAddingTrimEndTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < VideoService.Metadata.Duration && parsedTimeSpan > TimeSpan.Zero) TrimEnd = parsedTimeSpan;
                else
                {
                    TrimEnd = VideoService.Metadata.Duration;
                    HomeAddingTrimEndTextBox.Text = TimeSpanCustomFormat.ToOptTHMMBaseSS(TrimEnd);
                }
            }
        }

        // FILENAME TEXTBOX LOST FOCUS
        private void HomeAddingFilenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) HomeAddingFilenameTextBox.Text = HomeAddingFilenameTextBox.Text.Replace(c, ' ');
            Filename = HomeAddingFilenameTextBox.Text;
        }

        // EXTENSION COMBOBOX SELECTION CHANGED
        private void HomeAddingExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Extension = (MediaFileExtension)HomeAddingExtensionComboBox.SelectedIndex + (MediaType == MediaType.OnlyAudio ? 3 : 0);
        }

        // SCHEDULE NUMBERBOX LOST FOCUS
        private void HomeAddingScheduleNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HomeAddingScheduleNumberBox.Value == double.NaN) HomeAddingScheduleNumberBox.Value = 0;
            Schedule = HomeAddingScheduleNumberBox.Value;
        }

        // LOCATION BROWSE BUTTON CLICKED
        private async void HomeAddingLocationBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Create location picker
            FolderPicker picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");

            // Select location
            StorageFolder selectedFolder = await picker.PickSingleFolderAsync();

            if (selectedFolder != null)
            {
                try
                {
                    await(await selectedFolder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("last_media_location", selectedFolder);
                    Location = selectedFolder;
                    HomeAddingLocationSettingControl.Description = Location.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        #endregion
    }
}
