using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Core.Enums;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Home
{
    public sealed partial class HomePlaylistAddingPanelVideoPanel : UserControl
    {
        #region CONSTANTS

        private readonly ResourceDictionary ImagesRes = new ResourceDictionary { Source = new Uri("ms-appx:///Resources/Images.xaml") };

        #endregion



        #region CONSTRUCTORS

        public HomePlaylistAddingPanelVideoPanel(IVideoService videoService)
        {
            this.InitializeComponent();

            // Set video service object
            VideoService = videoService;

            // Set metadata
            ThumbnailImage = VideoService.Thumbnail != null ? new BitmapImage { UriSource = VideoService.Thumbnail } : (BitmapImage)ImagesRes["UnknownThumbnailImage"];
            SourceImage = new BitmapIcon { UriSource = new Uri($"ms-appx:///Assets/Sources/{VideoService.GetType().Namespace.Split(".").Last()}.png"), ShowAsMonochrome = false };
            Title = VideoService.Title;
            Author = VideoService.Author;
            Views = VideoService.Views.ToString();
            Date = VideoService.Date.ToString(CultureInfo.InstalledUICulture.DateTimeFormat.ShortDatePattern);
            Duration = $"{(Math.Floor(VideoService.Duration.TotalHours) > 0 ? $"{Math.Floor(VideoService.Duration.TotalHours):0}:" : "")}{VideoService.Duration.Minutes:00}:{VideoService.Duration.Seconds:00}";

            // Set media type
            foreach (string mediaType in Enum.GetNames(typeof(MediaType)))
            {
                HomePlaylistAddingVideoPanelMediaTypeSettingControlComboBox.Items.Add(ResourceLoader.GetForCurrentView().GetString($"MediaType{mediaType}Text"));
            }
            HomePlaylistAddingVideoPanelMediaTypeSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_media_type");

            // Set quality
            foreach (IBaseStream stream in VideoService.BaseStreams)
            {
                HomePlaylistAddingVideoPanelQualitySettingControlComboBox.Items.Add($"{stream.Height}p{(stream.FrameRate > 0 ? stream.FrameRate.ToString() : "N/A")}");
            }
            HomePlaylistAddingVideoPanelQualitySettingControlComboBox.SelectedIndex = 0;

            // Set trim start
            if (Math.Floor(VideoService.Duration.TotalHours) > 0) HomePlaylistAddingVideoPanelTrimStartTextBox.Text += $"{new string('0', Math.Floor(VideoService.Duration.TotalHours).ToString().Length)}:";
            if (Math.Floor(VideoService.Duration.TotalMinutes) > 0) HomePlaylistAddingVideoPanelTrimStartTextBox.Text += Math.Floor(VideoService.Duration.TotalHours) > 0 ? "00:" : $"{new string('0', VideoService.Duration.Minutes.ToString().Length)}:";
            HomePlaylistAddingVideoPanelTrimStartTextBox.Text += Math.Floor(VideoService.Duration.TotalMinutes) > 0 ? "00" : $"{new string('0', VideoService.Duration.Seconds.ToString().Length)}";

            // Set trim end
            if (Math.Floor(VideoService.Duration.TotalHours) > 0) HomePlaylistAddingVideoPanelTrimEndTextBox.Text += $"{Math.Floor(VideoService.Duration.TotalHours)}:";
            if (Math.Floor(VideoService.Duration.TotalMinutes) > 0) HomePlaylistAddingVideoPanelTrimEndTextBox.Text += Math.Floor(VideoService.Duration.TotalHours) > 0 ? $"{VideoService.Duration.Minutes:00}:" : $"{VideoService.Duration.Minutes}:";
            HomePlaylistAddingVideoPanelTrimEndTextBox.Text += Math.Floor(VideoService.Duration.TotalMinutes) > 0 ? $"{VideoService.Duration.Seconds:00}" : $"{VideoService.Duration.Seconds}";

            // Set filename
            string temporaryFilename = (string)Config.GetValue("default_filename");
            Dictionary<string, string> filenameStandardTemplates = new Dictionary<string, string>()
            {
                { "<title>", VideoService.Title },
                { "<author>", VideoService.Author },
                { "<views>", VideoService.Views.ToString() },
                { "<id>", VideoService.ID },
            };
            foreach (KeyValuePair<string, string> template in filenameStandardTemplates) temporaryFilename = temporaryFilename.Replace(template.Key, template.Value);
            Dictionary<Regex, IFormattable> filenameFormatTemplates = new Dictionary<Regex, IFormattable>()
            {
                { new Regex(@"<date_pub:(?<format>.*)>"), VideoService.Date },
                { new Regex(@"<date_now:(?<format>.*)>"), DateTime.Now },
                { new Regex(@"<duration:(?<format>.*)>"), VideoService.Duration },
            };
            foreach (KeyValuePair<Regex, IFormattable> template in filenameFormatTemplates) foreach (Match templateMatch in template.Key.Matches(temporaryFilename)) temporaryFilename = temporaryFilename.Replace(templateMatch.Value, template.Value.ToString(templateMatch.Groups["format"].Value, null));
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) temporaryFilename = temporaryFilename.Replace(c, ' ');
            HomePlaylistAddingVideoPanelFilenameTextBox.Text = temporaryFilename;
            Filename = temporaryFilename;

            // Set location
            if (!(bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("last_media_location"))
            {
                Task<StorageFolder> task = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location").AsTask();
                Location = task.Result;
                HomePlaylistAddingVideoPanelLocationSettingControl.Description = Location.Path;
            }
            else if ((bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                Task<StorageFolder> task = StorageApplicationPermissions.FutureAccessList.GetFolderAsync("selected_media_location").AsTask();
                Location = task.Result;
                HomePlaylistAddingVideoPanelLocationSettingControl.Description = Location.Path;
            }
            else
            {
                Location = null;
                HomePlaylistAddingVideoPanelLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }

            // Set minutes to start
            Schedule = 0;
        }

        #endregion



        #region PROPERTIES

        // BASE VIDEO DATA
        public IVideoService VideoService { get; set; }

        // VIDEO DATA
        private ImageSource ThumbnailImage { get; set; }
        private IconElement SourceImage { get; set; }
        private string Title { get; set; }
        private string Author { get; set; }
        private string Views { get; set; }
        private string Date { get; set; }
        private string Duration { get; set; }

        // VIDEO OPTIONS
        public MediaType MediaType { get; set; }
        public IBaseStream Stream { get; set; }
        public TimeSpan TrimStart { get; set; }
        public TimeSpan TrimEnd { get; set; }
        public string Filename { get; set; }
        public MediaFileExtension Extension { get; set; }
        public StorageFolder Location { get; set; }
        public double Schedule { get; set; }

        #endregion



        #region EVENT HANDLERS VOIDS

        // MEDIA TYPE COMBOBOX SELECTION CHANGED
        private void HomePlaylistAddingVideoPanelMediaTypeSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaType = (MediaType)HomePlaylistAddingVideoPanelMediaTypeSettingControlComboBox.SelectedIndex;
            if (HomePlaylistAddingVideoPanelMediaTypeSettingControlComboBox.SelectedIndex == (int)MediaType.OnlyAudio)
            {
                HomePlaylistAddingVideoPanelQualitySettingControl.Visibility = Visibility.Collapsed;
                HomePlaylistAddingVideoPanelQualitySettingControlComboBox.SelectedIndex = VideoService.BaseStreams.Count() - 1;

                HomePlaylistAddingVideoPanelExtensionComboBox.Items.Clear();
                foreach (AudioFileExtension extension in Enum.GetValues(typeof(AudioFileExtension)))
                {
                    HomePlaylistAddingVideoPanelExtensionComboBox.Items.Add(extension);
                }
                HomePlaylistAddingVideoPanelExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_audio_extension") - 3;
            }
            else
            {
                HomePlaylistAddingVideoPanelQualitySettingControl.Visibility = Visibility.Visible;
                HomePlaylistAddingVideoPanelQualitySettingControlComboBox.SelectedIndex = 0;

                HomePlaylistAddingVideoPanelExtensionComboBox.Items.Clear();
                foreach (VideoFileExtension extension in Enum.GetValues(typeof(VideoFileExtension)))
                {
                    HomePlaylistAddingVideoPanelExtensionComboBox.Items.Add(extension);
                }
                HomePlaylistAddingVideoPanelExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_video_extension");
            }
        }

        // QUALITY COMBOBOX SELECTION CHANGED
        private void HomePlaylistAddingVideoPanelQualitySettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stream = VideoService.BaseStreams[HomePlaylistAddingVideoPanelQualitySettingControlComboBox.SelectedIndex];
        }

        // TRIM START TEXTBOX TEXT CHANGED
        private void HomePlaylistAddingVideoPanelTrimStartTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!HomePlaylistAddingVideoPanelTrimStartTextBox.Text.Contains('_'))
            {
                string[] segments = HomePlaylistAddingVideoPanelTrimStartTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < VideoService.Duration && parsedTimeSpan > new TimeSpan(0)) TrimStart = parsedTimeSpan;
                else
                {
                    TrimStart = new TimeSpan(0);

                    string newText = string.Empty;
                    if (Math.Floor(VideoService.Duration.TotalHours) > 0) newText += $"{new string('0', Math.Floor(VideoService.Duration.TotalHours).ToString().Length)}:";
                    if (Math.Floor(VideoService.Duration.TotalMinutes) > 0) newText += Math.Floor(VideoService.Duration.TotalHours) > 0 ? "00:" : $"{new string('0', VideoService.Duration.Minutes.ToString().Length)}:";
                    newText += Math.Floor(VideoService.Duration.TotalMinutes) > 0 ? "00" : $"{new string('0', VideoService.Duration.Seconds.ToString().Length)}";

                    if (newText != HomePlaylistAddingVideoPanelTrimStartTextBox.Text) HomePlaylistAddingVideoPanelTrimStartTextBox.Text = newText;
                }
            }
        }

        // TRIM END TEXTBOX TEXT CHANGED
        private void HomePlaylistAddingVideoPanelTrimEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!HomePlaylistAddingVideoPanelTrimEndTextBox.Text.Contains('_'))
            {
                string[] segments = HomePlaylistAddingVideoPanelTrimEndTextBox.Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < VideoService.Duration && parsedTimeSpan > new TimeSpan(0)) TrimEnd = parsedTimeSpan;
                else
                {
                    TrimEnd = VideoService.Duration;

                    string newText = string.Empty;
                    if (Math.Floor(VideoService.Duration.TotalHours) > 0) newText += $"{Math.Floor(VideoService.Duration.TotalHours)}:";
                    if (Math.Floor(VideoService.Duration.TotalMinutes) > 0) newText += Math.Floor(VideoService.Duration.TotalHours) > 0 ? $"{TrimEnd.Minutes:00}:" : $"{TrimEnd.Minutes}:";
                    newText += Math.Floor(VideoService.Duration.TotalMinutes) > 0 ? $"{TrimEnd.Seconds:00}" : $"{TrimEnd.Seconds}";

                    if (newText != HomePlaylistAddingVideoPanelTrimEndTextBox.Text) HomePlaylistAddingVideoPanelTrimEndTextBox.Text = newText;
                }
            }
        }

        // FILENAME TEXTBOX TEXT CHANGED
        private void HomePlaylistAddingVideoPanelFilenameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string oldFilename = HomePlaylistAddingVideoPanelFilenameTextBox.Text;
            string newFilename = oldFilename;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) newFilename = newFilename.Replace(c, ' ');
            if (oldFilename != newFilename)
            {
                HomePlaylistAddingVideoPanelFilenameTextBox.Text = newFilename;
                Filename = newFilename;
            }
        }

        // EXTENSION COMBOBOX SELECTION CHANGED
        private void HomePlaylistAddingVideoPanelExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Extension = (MediaFileExtension)HomePlaylistAddingVideoPanelExtensionComboBox.SelectedIndex + (MediaType == MediaType.OnlyAudio ? 3 : 0);
        }

        // SCHEDULE NUMBERBOX VALUE CHANGED
        private void HomePlaylistAddingVideoPanelScheduleNumberBox_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            Schedule = HomePlaylistAddingVideoPanelScheduleNumberBox.Value;
        }

        // LOCATION BROWSE BUTTON CLICKED
        private async void HomePlaylistAddingVideoPanelLocationBrowseButton_Click(object sender, RoutedEventArgs e)
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
                    Location = selectedFolder;
                    HomePlaylistAddingVideoPanelLocationSettingControl.Description = Location.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        // SOURCE BUTTON CLICKED
        private async void HomePlaylistAddingVideoPanelSourceButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch the website
            await Windows.System.Launcher.LaunchUriAsync(VideoService.VideoUrl);
        }

        // DELETE BUTTON CLICKED
        private void HomePlaylistAddingVideoPanelDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler DeleteRequested;

        #endregion
    }
}
