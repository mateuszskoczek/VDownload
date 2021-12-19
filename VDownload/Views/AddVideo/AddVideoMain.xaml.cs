using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Services;
using VDownload.Sources;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.Views.AddVideo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddVideoMain : Page
    {
        #region INIT

        // VIDEO OBJECT
        public static VObject Video;

        // CONSTRUCTOR
        public AddVideoMain()
        {
            InitializeComponent();
        }

        #endregion

        

        #region MAIN
        
        // NAVIGATED TO THIS PAGE
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get video object from parent
            base.OnNavigatedTo(e);
            Video = (VObject)e.Parameter;

            // Set icons theme
            AddVideoVideoDataAuthorIconImage.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Author.png") };
            AddVideoVideoDataViewsIconImage.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Views.png") };
            AddVideoVideoDataDateIconImage.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Date.png") };
            AddVideoVideoDataDurationIconImage.Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Duration.png") };

            // Set source image 
            AddVideoVideoDataSourceIconImage.Source = new BitmapImage(Video.SourceIcon);

            // Set title
            AddVideoVideoDataTitleTextBlock.Text = Video.Title;

            // Set thumbnail
            try
            {
                AddVideoVideoDataThumbnailImage.Source = new BitmapImage(Video.Thumbnail);
            }
            catch { }

            // Set metadata
            AddVideoVideoDataAuthorTextBlock.Text = Video.Author;
            AddVideoVideoDataViewsTextBlock.Text = Video.Views.ToString();
            AddVideoVideoDataDateTextBlock.Text = Video.Date.ToString((string)Config.GetValue("date_format"));
            AddVideoVideoDataDurationTextBlock.Text = Video.Duration.ToString();

            // Set items in quality combobox
            foreach (string q in Video.Streams.Keys)
            {
                AddVideoDownloadOptionsQualityComboBox.Items.Add(q);
            }

            // Set items in extension combobox
            foreach (string x in Video.MediaType == "A" ? Config.DefaultAudioExtensionList : Config.DefaultVideoExtensionList)
            {
                AddVideoFileDataExtensionComboBox.Items.Add(x);
            }

            // Set quality option
            if (Video.MediaType == "A")
            {
                AddVideoDownloadOptionsQualityComboBox.SelectedValue = ResourceLoader.GetForCurrentView().GetString("AddVideoQualityNoVideoStream");
                AddVideoDownloadOptionsQualityComboBox.IsEnabled = false;
            }
            else
            {
                AddVideoDownloadOptionsQualityComboBox.SelectedValue = Video.SelectedQuality;
                AddVideoDownloadOptionsQualityComboBox.IsEnabled = true;
            }

            // Set trim options
            AddVideoDownloadOptionsTrimStartTextBox.Text = Video.TrimStart.ToString();
            AddVideoDownloadOptionsTrimEndTextBox.Text = Video.TrimEnd.ToString();

            // Set media type option
            switch (Video.MediaType)
            {
                case "AV": AddVideoDownloadOptionsMediaTypeRadiobuttonAV.IsChecked = true; break;
                case "V": AddVideoDownloadOptionsMediaTypeRadiobuttonV.IsChecked = true; break;
                case "A": AddVideoDownloadOptionsMediaTypeRadiobuttonA.IsChecked = true; break;
                default: AddVideoDownloadOptionsMediaTypeRadiobuttonAV.IsChecked = true; break;
            }

            // Set filename option
            AddVideoFileDataFilenameTextBox.Text = Video.Filename;

            // Set extension option
            AddVideoFileDataExtensionComboBox.SelectedItem = Video.Extension;

            // Set location option
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("save"))
            {
                Video.CustomSaveLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("save");
                AddVideoLocationDataLocationTextBlock.Text = Video.CustomSaveLocation.Path;
                Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
            }
            else
            {
                AddVideoLocationDataLocationTextBlock.Text = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\";
                Video.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Video.Filename}.{Video.Extension.ToLower()}";
            }
        }

        #endregion



        #region VALUE CHANGED HANDLERS

        // MEDIA TYPE RADIOBUTTON (AV)
        private void AddVideoDownloadOptionsMediaTypeRadiobuttonAV_Checked(object sender, RoutedEventArgs e)
        {
            Video.MediaType = "AV";
            AddVideoDownloadOptionsQualityComboBox.IsEnabled = true;
            AddVideoDownloadOptionsQualityComboBox.SelectedValue = Video.Streams.Keys.ToArray()[0];
            AddVideoFileDataExtensionComboBox.Items.Clear();
            foreach (string x in Config.DefaultVideoExtensionList)
            {
                AddVideoFileDataExtensionComboBox.Items.Add(x);
            }
            AddVideoFileDataExtensionComboBox.SelectedItem = Config.GetValue("default_video_extension");
        }

        // MEDIA TYPE RADIOBUTTON (A)
        private void AddVideoDownloadOptionsMediaTypeRadiobuttonA_Checked(object sender, RoutedEventArgs e)
        {
            Video.MediaType = "A";
            AddVideoDownloadOptionsQualityComboBox.IsEnabled = false;
            AddVideoDownloadOptionsQualityComboBox.SelectedValue = ResourceLoader.GetForCurrentView().GetString("AddVideoQualityNoVideoStream");
            AddVideoFileDataExtensionComboBox.Items.Clear();
            foreach (string x in Config.DefaultAudioExtensionList)
            {
                AddVideoFileDataExtensionComboBox.Items.Add(x);
            }
            AddVideoFileDataExtensionComboBox.SelectedItem = Config.GetValue("default_audio_extension");
        }

        // MEDIA TYPE RADIOBUTTON (V)
        private void AddVideoDownloadOptionsMediaTypeRadiobuttonV_Checked(object sender, RoutedEventArgs e)
        {
            Video.MediaType = "V";
            AddVideoDownloadOptionsQualityComboBox.IsEnabled = true;
            AddVideoDownloadOptionsQualityComboBox.SelectedValue = Video.Streams.Keys.ToArray()[0];
            AddVideoFileDataExtensionComboBox.Items.Clear();
            foreach (string x in Config.DefaultVideoExtensionList)
            {
                AddVideoFileDataExtensionComboBox.Items.Add(x);
            }
            AddVideoFileDataExtensionComboBox.SelectedItem = Config.GetValue("default_video_extension");
        }

        // QUALITY COMBOBOX
        private void AddVideoDownloadOptionsQualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Video.MediaType == "A")
                Video.SelectedQuality = Video.Streams.Keys.ToArray()[0];
            else
                Video.SelectedQuality = (string)AddVideoDownloadOptionsQualityComboBox.SelectedItem;
        }

        // TRIM START TEXTBOX
        private void AddVideoDownloadOptionsTrimStartTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Video.TrimStart = TimeSpan.Parse(AddVideoDownloadOptionsTrimStartTextBox.Text);
            }
            catch { }
        }

        // TRIM END TEXTBOX
        private void AddVideoDownloadOptionsTrimEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Video.TrimEnd = TimeSpan.Parse(AddVideoDownloadOptionsTrimEndTextBox.Text);
            }
            catch { }
        }

        // FILENAME TEXTBOX
        private void AddVideoFileDataFilenameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                AddVideoFileDataFilenameTextBox.Text = AddVideoFileDataFilenameTextBox.Text.Replace(c, ' ');
            }
            Video.Filename = AddVideoFileDataFilenameTextBox.Text;
            if (Video.CustomSaveLocation != null)
                Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
            else
                Video.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Video.Filename}.{Video.Extension.ToLower()}";
        }

        // EXTENSION COMBOBOX
        private void AddVideoFileDataExtensionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(AddVideoFileDataExtensionComboBox.SelectedItem == null))
            {
                Video.Extension = (string)AddVideoFileDataExtensionComboBox.SelectedItem;
                if (Video.CustomSaveLocation != null)
                    Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
                else
                    Video.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Video.Filename}.{Video.Extension.ToLower()}";
            }
        }

        #endregion



        #region BUTTON CLICK HANDLERS

        // CHOOSE LOCATION BUTTON
        private async void AddVideoLocationDataChooseLocationButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                try
                {
                    await (await folder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    Video.CustomSaveLocation = folder;
                    AddVideoLocationDataLocationTextBlock.Text = Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\';
                    Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
                }
                catch { }
            }
        }

        #endregion
    }
}
