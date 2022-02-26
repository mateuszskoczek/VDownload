using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgsObjects;
using VDownload.Core.Interfaces;
using VDownload.Core.Objects;
using VDownload.Core.Services;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace VDownload.Views.Home
{
    public sealed partial class HomeVideoAddingPanel : UserControl
    {
        #region CONSTRUCTORS

        public HomeVideoAddingPanel(IVideoService videoData)
        {
            this.InitializeComponent();

            // Set video service object
            VideoData = videoData;

            // Set metadata
            ThumbnailImage = new BitmapImage { UriSource = VideoData.Thumbnail ?? new Uri("ms-appx:///Assets/UnknownThumbnail.png") };
            SourceImage = new BitmapIcon { ShowAsMonochrome = false, UriSource = new Uri($"ms-appx:///Assets/{VideoData.GetType().Namespace.Split(".").Last()}.png") };
            Title = VideoData.Title;
            Author = VideoData.Author;
            Views = VideoData.Views.ToString();
            Date = VideoData.Date.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern);
            Duration = $"{Math.Floor(VideoData.Duration.TotalHours):00}:{VideoData.Duration.Minutes:00}:{VideoData.Duration.Seconds:00}";


            // Set media type
            foreach (string mediaType in Enum.GetNames(typeof(MediaType)))
            {
                HomeVideoAddingMediaTypeSettingControlComboBox.Items.Add(ResourceLoader.GetForCurrentView().GetString($"MediaType{mediaType}Text"));
            }
            HomeVideoAddingMediaTypeSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_media_type");

            // Set quality
            foreach (Stream stream in VideoData.Streams)
            {
                HomeVideoAddingQualitySettingControlComboBox.Items.Add($"{stream.Height}p{(stream.FrameRate > 0 ? stream.FrameRate.ToString() : "N/A")}");
            }
            HomeVideoAddingQualitySettingControlComboBox.SelectedIndex = 0;

            // Set trim start
            if (Math.Floor(VideoData.Duration.TotalHours) > 0) HomeVideoAddingTrimStartTextBox.Text += $"{new string('0', Math.Floor(VideoData.Duration.TotalHours).ToString().Length)}:";
            if (Math.Floor(VideoData.Duration.TotalMinutes) > 0) HomeVideoAddingTrimStartTextBox.Text += Math.Floor(VideoData.Duration.TotalHours) > 0 ? "00:" : $"{new string('0', VideoData.Duration.Minutes.ToString().Length)}:";
            HomeVideoAddingTrimStartTextBox.Text += Math.Floor(VideoData.Duration.TotalMinutes) > 0 ? "00" : $"{new string('0', VideoData.Duration.Seconds.ToString().Length)}";

            // Set trim end
            if (Math.Floor(VideoData.Duration.TotalHours) > 0) HomeVideoAddingTrimEndTextBox.Text += $"{Math.Floor(VideoData.Duration.TotalHours)}:";
            if (Math.Floor(VideoData.Duration.TotalMinutes) > 0) HomeVideoAddingTrimEndTextBox.Text += Math.Floor(VideoData.Duration.TotalHours) > 0 ? $"{VideoData.Duration.Minutes:00}:" : $"{VideoData.Duration.Minutes}:";
            HomeVideoAddingTrimEndTextBox.Text += Math.Floor(VideoData.Duration.TotalMinutes) > 0 ? $"{VideoData.Duration.Seconds:00}" : $"{VideoData.Duration.Seconds}";

            // Set filename
            string temporaryFilename = (string)Config.GetValue("default_filename");
            Dictionary<string, string> filenameStandardTemplates = new Dictionary<string, string>()
            {
                { "<title>", VideoData.Title },
                { "<author>", VideoData.Author },
                { "<views>", VideoData.Views.ToString() },
                { "<id>", VideoData.ID },
            };
            foreach (KeyValuePair<string, string> template in filenameStandardTemplates) temporaryFilename = temporaryFilename.Replace(template.Key, template.Value);
            Dictionary<Regex, IFormattable> filenameFormatTemplates = new Dictionary<Regex, IFormattable>()
            {
                { new Regex(@"<date_pub:(?<format>.*)>"), VideoData.Date },
                { new Regex(@"<date_now:(?<format>.*)>"), DateTime.Now },
                { new Regex(@"<duration:(?<format>.*)>"), VideoData.Duration },
            };
            foreach (KeyValuePair<Regex, IFormattable> template in filenameFormatTemplates) foreach (Match templateMatch in template.Key.Matches(temporaryFilename)) temporaryFilename = temporaryFilename.Replace(templateMatch.Value, template.Value.ToString(templateMatch.Groups["format"].Value, null));
            HomeVideoAddingFilenameTextBox.Text = temporaryFilename;

            // Set location
            if ((DefaultLocationType)Config.GetValue("default_location_type") == DefaultLocationType.Last && StorageApplicationPermissions.FutureAccessList.ContainsItem("last_media_location"))
            {
                Task<StorageFolder> task = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location").AsTask();
                Location = task.Result;
                HomeVideoAddingLocationSettingControl.Description = Location.Path;
            }
            else if ((DefaultLocationType)Config.GetValue("default_location_type") == DefaultLocationType.Selected && StorageApplicationPermissions.FutureAccessList.ContainsItem("selected_media_location"))
            {
                Task<StorageFolder> task = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("selected_media_location").AsTask();
                Location = task.Result;
                HomeVideoAddingLocationSettingControl.Description = Location.Path;
            }
            else
            {
                Location = null;
                HomeVideoAddingLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }
        }

        #endregion



        #region PROPERTIES

        // BASE VIDEO DATA
        private IVideoService VideoData { get; set; }

        // VIDEO DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Views { get; set; }
        private string Date { get; set; }
        private string Duration { get; set; }

        // VIDEO OPTIONS
        private MediaType MediaType { get; set; }
        private Stream Stream { get; set; }
        private TimeSpan TrimStart { get; set; }
        private TimeSpan TrimEnd { get; set; }
        private string Filename { get; set; }
        private MediaFileExtension Extension { get; set; }
        private StorageFolder Location { get; set; }

        #endregion



        #region EVENT HANDLERS VOIDS

        // MEDIA TYPE COMBOBOX SELECTION CHANGED
        private void HomeVideoAddingMediaTypeSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaType = (MediaType)HomeVideoAddingMediaTypeSettingControlComboBox.SelectedIndex;
            if (HomeVideoAddingMediaTypeSettingControlComboBox.SelectedIndex == (int)MediaType.OnlyAudio)
            {
                HomeVideoAddingQualitySettingControl.Visibility = Visibility.Collapsed;
                HomeVideoAddingQualitySettingControlComboBox.SelectedIndex = VideoData.Streams.Count() - 1;

                HomeVideoAddingExtensionComboBox.Items.Clear();
                foreach (AudioFileExtension extension in Enum.GetValues(typeof(AudioFileExtension)))
                {
                    HomeVideoAddingExtensionComboBox.Items.Add(extension);
                }
                HomeVideoAddingExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_audio_extension") - 3;
            }
            else
            {
                HomeVideoAddingQualitySettingControl.Visibility = Visibility.Visible;
                HomeVideoAddingQualitySettingControlComboBox.SelectedIndex = 0;

                HomeVideoAddingExtensionComboBox.Items.Clear();
                foreach (VideoFileExtension extension in Enum.GetValues(typeof(VideoFileExtension)))
                {
                    HomeVideoAddingExtensionComboBox.Items.Add(extension);
                }
                HomeVideoAddingExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_video_extension");
            }
        }

        // QUALITY COMBOBOX SELECTION CHANGED
        private void HomeVideoAddingQualitySettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stream = VideoData.Streams[HomeVideoAddingQualitySettingControlComboBox.SelectedIndex];
        }

        // TRIM START TEXTBOX TEXT CHANGED
        private void HomeVideoAddingTrimStartTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!HomeVideoAddingTrimStartTextBox.Text.Contains('_'))
            {
                string[] segments = HomeVideoAddingTrimStartTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < VideoData.Duration && parsedTimeSpan > new TimeSpan(0)) TrimStart = parsedTimeSpan;
                else
                {
                    TrimStart = new TimeSpan(0);

                    string newText = string.Empty;
                    if (Math.Floor(VideoData.Duration.TotalHours) > 0) newText += $"{new string('0', Math.Floor(VideoData.Duration.TotalHours).ToString().Length)}:";
                    if (Math.Floor(VideoData.Duration.TotalMinutes) > 0) newText += Math.Floor(VideoData.Duration.TotalHours) > 0 ? "00:" : $"{new string('0', VideoData.Duration.Minutes.ToString().Length)}:";
                    newText += Math.Floor(VideoData.Duration.TotalMinutes) > 0 ? "00" : $"{new string('0', VideoData.Duration.Seconds.ToString().Length)}";

                    if (newText != HomeVideoAddingTrimStartTextBox.Text) HomeVideoAddingTrimStartTextBox.Text = newText;
                }
            }
        }

        // TRIM END TEXTBOX TEXT CHANGED
        private void HomeVideoAddingTrimEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!HomeVideoAddingTrimEndTextBox.Text.Contains('_'))
            {
                string[] segments = HomeVideoAddingTrimEndTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < VideoData.Duration && parsedTimeSpan > new TimeSpan(0)) TrimEnd = parsedTimeSpan;
                else
                {
                    TrimEnd = VideoData.Duration;

                    string newText = string.Empty;
                    if (Math.Floor(VideoData.Duration.TotalHours) > 0) newText += $"{Math.Floor(VideoData.Duration.TotalHours)}:";
                    if (Math.Floor(VideoData.Duration.TotalMinutes) > 0) newText += Math.Floor(VideoData.Duration.TotalHours) > 0 ? $"{TrimEnd.Minutes:00}:" : $"{TrimEnd.Minutes}:";
                    newText += Math.Floor(VideoData.Duration.TotalMinutes) > 0 ? $"{TrimEnd.Seconds:00}" : $"{TrimEnd.Seconds}";

                    if (newText != HomeVideoAddingTrimEndTextBox.Text) HomeVideoAddingTrimEndTextBox.Text = newText;
                }
            }
        }

        // FILENAME TEXTBOX TEXT CHANGED
        private void HomeVideoAddingFilenameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldFilename = HomeVideoAddingFilenameTextBox.Text;
            string newFilename = oldFilename;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) newFilename = newFilename.Replace(c, ' ');
            if (oldFilename != newFilename) HomeVideoAddingFilenameTextBox.Text = newFilename;
        }

        // EXTENSION COMBOBOX SELECTION CHANGED
        private void HomeVideoAddingExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Extension = (MediaFileExtension)HomeVideoAddingExtensionComboBox.SelectedIndex + (MediaType == MediaType.OnlyAudio ? 3 : 0);
        }

        // LOCATION BROWSE BUTTON CLICKED
        private async void HomeVideoAddingLocationBrowseButton_Click(object sender, RoutedEventArgs e)
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
                    Location = selectedFolder;
                    HomeVideoAddingLocationSettingControl.Description = Location.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        // SOURCE BUTTON CLICKED
        public async void HomeVideoAddingPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(VideoData.VideoUrl);
        }

        // ADD BUTTON CLICKED
        public void HomeVideoAddingPanelAddButton_Click(object sender, RoutedEventArgs e)
        {
            VideoAddEventArgs args = new VideoAddEventArgs
            {
                VideoService = VideoData,
                MediaType = MediaType,
                Stream = Stream,
                TrimStart = TrimStart,
                TrimEnd = TrimEnd,
                Filename = Filename,
                Extension = Extension,
                Location = Location,
            };
            VideoAddRequest?.Invoke(this, args);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<VideoAddEventArgs> VideoAddRequest;

        #endregion
    }
}
