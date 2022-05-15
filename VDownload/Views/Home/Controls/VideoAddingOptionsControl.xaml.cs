using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Controls;
using VDownload.Core.Enums;
using VDownload.Core.Extensions;
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
    public sealed partial class VideoAddingOptionsControl : UserControl
    {
        #region CONSTRUCTORS

        public VideoAddingOptionsControl(IVideo video)
        {
            this.InitializeComponent();

            Video = video;

            foreach (string mediaType in Enum.GetNames(typeof(MediaType)))
            {
                MediaTypeSettingControlComboBox.Items.Add(ResourceLoader.GetForCurrentView().GetString($"Base_MediaType_{mediaType}Text"));
            }
            MediaTypeSettingControlComboBox.SelectedIndex = (int)Config.GetValue("default_media_type");

            foreach (BaseStream stream in Video.BaseStreams)
            {
                QualitySettingControlComboBox.Items.Add(stream.ToString());
            }
            QualitySettingControlComboBox.SelectedIndex = 0;

            TrimStart = TimeSpan.Zero;
            TrimSettingControlStartTextBox.Text = TrimStart.ToStringOptTHMMBaseSS(Video.Duration);

            TrimEnd = Video.Duration;
            TrimSettingControlEndTextBox.Text = TrimEnd.ToStringOptTHMMBaseSS();

            string temporaryFilename = (string)Config.GetValue("default_filename");
            Dictionary<string, string> filenameStandardTemplates = new Dictionary<string, string>()
            {
                { "<title>", Video.Title },
                { "<author>", Video.Author },
                { "<views>", Video.Views.ToString() },
                { "<id>", Video.ID },
            };
            foreach (KeyValuePair<string, string> template in filenameStandardTemplates)
            {
                temporaryFilename = temporaryFilename.Replace(template.Key, template.Value);
            }
            Dictionary<Regex, IFormattable> filenameFormatTemplates = new Dictionary<Regex, IFormattable>()
            {
                { new Regex(@"<date_pub:(?<format>.*)>"), Video.Date },
                { new Regex(@"<date_now:(?<format>.*)>"), DateTime.Now },
                { new Regex(@"<duration:(?<format>.*)>"), Video.Duration },
            };
            foreach (KeyValuePair<Regex, IFormattable> template in filenameFormatTemplates)
            {
                foreach (Match templateMatch in template.Key.Matches(temporaryFilename))
                {
                    temporaryFilename = temporaryFilename.Replace(templateMatch.Value, template.Value.ToString(templateMatch.Groups["format"].Value, null));
                }
            }
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                temporaryFilename = temporaryFilename.Replace(c, ' ');
            }
            Filename = temporaryFilename;
            FileSettingControlFilenameTextBox.Text = Filename;

            Schedule = 0;
        }

        #endregion



        #region PROPERTIES

        private IVideo Video { get; set; }

        public MediaType MediaType { get; private set; }
        public BaseStream Stream { get; private set; }
        public TimeSpan TrimStart { get; private set; }
        public TimeSpan TrimEnd { get; private set; }
        public string Filename { get; private set; }
        public MediaFileExtension FileExtension { get; private set; }
        public StorageFolder FileLocation { get; private set; }
        public double Schedule { get; private set; }

        #endregion



        #region PUBLIC METHODS

        public void ChangeLocation(StorageFolder location)
        {
            FileLocation = location;
            FileLocationSettingControl.Description = FileLocation.Path ?? $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
        }

        public void ChangeSchedule(double schedule)
        {
            Schedule = schedule;
            ScheduleSettingControlNumberBox.Value = Schedule;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task Init()
        {
            if (!(bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("last_media_location"))
            {
                FileLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("last_media_location");
                FileLocationSettingControl.Description = FileLocation.Path;
            }
            else if ((bool)Config.GetValue("custom_media_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_media_location"))
            {
                FileLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("custom_media_location");
                FileLocationSettingControl.Description = FileLocation.Path;
            }
            else
            {
                FileLocation = null;
                FileLocationSettingControl.Description = $@"{UserDataPaths.GetDefault().Downloads}\VDownload";
            }
        }

        #endregion



        #region EVENT HANDLERS

        private void MediaTypeSettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MediaType = (MediaType)((ComboBox)sender).SelectedIndex;
            if (((ComboBox)sender).SelectedIndex == (int)MediaType.OnlyAudio)
            {
                QualitySettingControl.Visibility = Visibility.Collapsed;
                QualitySettingControlComboBox.SelectedIndex = Video.BaseStreams.Count() - 1;

                FileSettingControlFileExtensionComboBox.Items.Clear();
                foreach (AudioFileExtension extension in Enum.GetValues(typeof(AudioFileExtension)))
                {
                    FileSettingControlFileExtensionComboBox.Items.Add(extension);
                }
                FileSettingControlFileExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_audio_extension") - 3;
            }
            else
            {
                QualitySettingControl.Visibility = Visibility.Visible;
                QualitySettingControlComboBox.SelectedIndex = 0;

                FileSettingControlFileExtensionComboBox.Items.Clear();
                foreach (VideoFileExtension extension in Enum.GetValues(typeof(VideoFileExtension)))
                {
                    FileSettingControlFileExtensionComboBox.Items.Add(extension);
                }
                FileSettingControlFileExtensionComboBox.SelectedIndex = (int)Config.GetValue("default_video_extension");
            }
        }

        private void QualitySettingControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Stream = Video.BaseStreams[((ComboBox)sender).SelectedIndex];
        }

        private void TrimSettingControlStartTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!((TextBox)sender).Text.Contains('_'))
            {
                string[] segments = ((TextBox)sender).Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < Video.Duration && parsedTimeSpan > TimeSpan.Zero) TrimStart = parsedTimeSpan;
                else
                {
                    TrimStart = TimeSpan.Zero;
                    ((TextBox)sender).Text = TrimStart.ToStringOptTHMMBaseSS(Video.Duration);
                }
            }
        }

        private void TrimSettingControlEndTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!((TextBox)sender).Text.Contains('_'))
            {
                string[] segments = ((TextBox)sender).Text.Split(':').Reverse().ToArray();
                int.TryParse(segments.ElementAtOrDefault(0), out int seconds);
                int.TryParse(segments.ElementAtOrDefault(1), out int minutes);
                int.TryParse(segments.ElementAtOrDefault(2), out int hours);

                TimeSpan parsedTimeSpan = new TimeSpan(hours, minutes, seconds);

                if (parsedTimeSpan < Video.Duration && parsedTimeSpan > TimeSpan.Zero) TrimEnd = parsedTimeSpan;
                else
                {
                    TrimEnd = Video.Duration;
                    ((TextBox)sender).Text = TrimEnd.ToStringOptTHMMBaseSS();
                }
            }
        }

        private void FileSettingControlFilenameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                ((TextBox)sender).Text = ((TextBox)sender).Text.Replace(c, ' ');
            } 
            Filename = ((TextBox)sender).Text;
        }

        private void FileSettingControlFileExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileExtension = (MediaFileExtension)((ComboBox)sender).SelectedIndex + (MediaType == MediaType.OnlyAudio ? 3 : 0);
        }

        private async void FileSettingControlFileLocationBrowseButton_Click(object sender, RoutedEventArgs e)
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
                    FileLocation = selectedFolder;
                    FileLocationSettingControl.Description = FileLocation.Path;
                }
                catch (UnauthorizedAccessException) { }
            }
        }

        private void ScheduleSettingControlNumberBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (((NumberBox)sender).Value == double.NaN)
            {
                ((NumberBox)sender).Value = 0;
            } 
            Schedule = ((NumberBox)sender).Value;
        }

        #endregion
    }
}
