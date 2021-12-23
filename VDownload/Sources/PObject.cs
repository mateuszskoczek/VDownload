using Microsoft.Toolkit.Uwp.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Objects.Enums;
using VDownload.Services;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VDownload.Sources
{
    public class PObject
    {
        #region INIT

        // PLAYLIST DATA
        public PlaylistSource SourceType { get; private set; }
        public string ID { get; private set; }
        public Dictionary<VObject, TextBlock> VObjects { get; private set; }
        private List<VObject> DeletedVObjects { get; set; }

        // PLAYLIST PANEL OBJECTS
        public StackPanel PlaylistPanel { get; set; }
        private Grid DeletedVideosPanel { get; set; }

        // CONSTRUCTOR
        public PObject(Uri url)
        {
            DeletedVObjects = new List<VObject>();
            VObjects = new Dictionary<VObject, TextBlock>();
            (SourceType, ID) = Source.GetPlaylistSourceData(url);
            if (SourceType == PlaylistSource.Null)
            {
                throw new ArgumentException();
            }
        }

        #endregion



        #region MAIN

        // GET VIDEOS
        public async Task GetVideos()
        {
            VObject[] videos = null;
            switch (SourceType)
            {
                case PlaylistSource.TwitchChannel:
                    videos = await Twitch.Channel.GetVideos(ID);
                    break;
                case PlaylistSource.Null:
                    throw new ArgumentException();
            }

            foreach(VObject video in videos)
            {
                VObjects.Add(video, null);
            }
        }

        #endregion



        #region VIDEO PANEL

        // INIT PLAYLIST PANEL
        public async Task InitPlaylistPanel()
        {
            // Add videos to panel
            foreach (VObject video in VObjects.Keys.ToArray())
            {
                await VideoPanelHandler(video);
            }
        }

        // VIDEO PANEL HANDLER
        private async Task VideoPanelHandler(VObject Video)
        {
            // Video panel
            Expander videoPanel = new Expander
            {
                Margin = new Thickness(0, 5, 0, 5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            };



            // Header
            Grid videoPanelHeader = new Grid
            {
                Margin = new Thickness(0, 15, 0, 15),
            };
            videoPanelHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            videoPanelHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });
            videoPanelHeader.ColumnDefinitions.Add(new ColumnDefinition());
            videoPanelHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });
            videoPanelHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            videoPanel.Header = videoPanelHeader;
            
            // Thumbnail
            Image thumbnailImage = new Image
            {
                Source = new BitmapImage { UriSource = Video.Thumbnail },
                Width = 120,
            };
            Grid.SetColumn(thumbnailImage, 0);
            videoPanelHeader.Children.Add(thumbnailImage);


            // Metadata grid
            Grid metadataGrid = new Grid();
            metadataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            metadataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            metadataGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(metadataGrid, 2);
            videoPanelHeader.Children.Add(metadataGrid);

            // Title & Source icon grid
            Grid titleSourceGrid = new Grid();
            titleSourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            titleSourceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            titleSourceGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(titleSourceGrid, 0);
            metadataGrid.Children.Add(titleSourceGrid);

            // Title textblock
            TextBlock titleTextBlock = new TextBlock
            {
                Text = Video.Title,
                FontWeight = FontWeights.Bold,
                FontSize = 15,
            };
            Grid.SetColumn(titleTextBlock, 2);
            titleSourceGrid.Children.Add(titleTextBlock);

            // Source icon image
            Image sourceIcon = new Image
            {
                Source = new BitmapImage { UriSource = Video.SourceIcon },
                Width = 15,
            };
            Grid.SetColumn(sourceIcon, 0);
            titleSourceGrid.Children.Add(sourceIcon);

            // Details grid
            Grid detailedMetadataGrid = new Grid();
            detailedMetadataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            detailedMetadataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            detailedMetadataGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            detailedMetadataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetRow(detailedMetadataGrid, 2);
            metadataGrid.Children.Add(detailedMetadataGrid);
            double iconSize = 12;
            double textSize = 10;

            // Author icon
            Image authorIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Author.png") },
                Width = iconSize,
            };
            Grid.SetColumn(authorIcon, 0);
            Grid.SetRow(authorIcon, 0);
            detailedMetadataGrid.Children.Add(authorIcon);

            // Author data textblock
            TextBlock authorDataTextBlock = new TextBlock
            {
                Text = Video.Author,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(authorDataTextBlock, 2);
            Grid.SetRow(authorDataTextBlock, 0);
            detailedMetadataGrid.Children.Add(authorDataTextBlock);

            // Views icon
            Image viewsIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Views.png") },
                Width = iconSize,
            };
            Grid.SetColumn(viewsIcon, 0);
            Grid.SetRow(viewsIcon, 2);
            detailedMetadataGrid.Children.Add(viewsIcon);

            // Views data textblock
            TextBlock viewsDataTextBlock = new TextBlock
            {
                Text = Video.Views.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(viewsDataTextBlock, 2);
            Grid.SetRow(viewsDataTextBlock, 2);
            detailedMetadataGrid.Children.Add(viewsDataTextBlock);

            // Date icon
            Image dateIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Date.png") },
                Width = iconSize,
            };
            Grid.SetColumn(dateIcon, 4);
            Grid.SetRow(dateIcon, 0);
            detailedMetadataGrid.Children.Add(dateIcon);

            // Date data textblock
            TextBlock dateDataTextBlock = new TextBlock
            {
                Text = Video.Date.ToString((string)Config.GetValue("date_format")),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(dateDataTextBlock, 6);
            Grid.SetRow(dateDataTextBlock, 0);
            detailedMetadataGrid.Children.Add(dateDataTextBlock);

            // Duration icon
            Image durationIcon = new Image
            {
                Source = new BitmapImage { UriSource = new Uri($"ms-appx:///Assets/Icons/Universal/{(Application.Current.RequestedTheme == ApplicationTheme.Dark ? "Dark" : "Light")}/Duration.png") },
                Width = iconSize,
            };
            Grid.SetColumn(durationIcon, 4);
            Grid.SetRow(durationIcon, 2);
            detailedMetadataGrid.Children.Add(durationIcon);

            // Duration data textblock
            TextBlock durationDataTextBlock = new TextBlock
            {
                Text = Video.Duration.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = textSize,
            };
            Grid.SetColumn(durationDataTextBlock, 6);
            Grid.SetRow(durationDataTextBlock, 2);
            detailedMetadataGrid.Children.Add(durationDataTextBlock);


            // Delete button
            AppBarButton deleteButton = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.Clear),
                Width = 40,
                Height = 48,
                VerticalAlignment = VerticalAlignment.Center,
            };
            deleteButton.Click += (object sender, RoutedEventArgs e) =>
            {
                PlaylistPanel.Children.Remove(videoPanel);
                VObjects.Remove(Video);
                DeletedVObjects.Add(Video);
                DeletedVideosPanelHandler();
            };
            Grid.SetColumn(deleteButton, 4);
            videoPanelHeader.Children.Add(deleteButton);



            // Content
            Grid content = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            videoPanel.Content = content;


            // Download options
            Grid downloadOptionsGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            downloadOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            downloadOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            downloadOptionsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(downloadOptionsGrid, 0);
            content.Children.Add(downloadOptionsGrid);

            // Media type
            Grid mediaTypeGrid = new Grid();
            mediaTypeGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mediaTypeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            mediaTypeGrid.RowDefinitions.Add(new RowDefinition());
            mediaTypeGrid.RowDefinitions.Add(new RowDefinition());
            mediaTypeGrid.RowDefinitions.Add(new RowDefinition());
            Grid.SetColumn(mediaTypeGrid, 0);
            downloadOptionsGrid.Children.Add(mediaTypeGrid);

            // Media type textblock
            TextBlock AddPlaylistVideoDownloadOptionsMediaTypeTextBlock = new TextBlock
            {
                Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoDownloadOptionsMediaTypeTextBlock"),
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsMediaTypeTextBlock, 0);
            mediaTypeGrid.Children.Add(AddPlaylistVideoDownloadOptionsMediaTypeTextBlock);

            // Media type radiobutton (AV)
            RadioButton AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV = new RadioButton
            {
                Content = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                GroupName = $"QualityRadiobuttons{Video.UniqueID}"
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV, 2);
            mediaTypeGrid.Children.Add(AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV);

            // Media type radiobutton (A)
            RadioButton AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonA = new RadioButton
            {
                Content = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonA"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                GroupName = $"QualityRadiobuttons{Video.UniqueID}"
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonA, 3);
            mediaTypeGrid.Children.Add(AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonA);

            // Media type radiobutton (V)
            RadioButton AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonV = new RadioButton
            {
                Content = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonV"),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                GroupName = $"QualityRadiobuttons{Video.UniqueID}"
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonV, 4);
            mediaTypeGrid.Children.Add(AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonV);

            // Separator
            AppBarSeparator AddPlaylistVideoDownloadOptionsSeparator = new AppBarSeparator
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetColumn(AddPlaylistVideoDownloadOptionsSeparator, 1);
            downloadOptionsGrid.Children.Add(AddPlaylistVideoDownloadOptionsSeparator);

            // Quality & Trim grid
            Grid qualityTrimGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            qualityTrimGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            qualityTrimGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            qualityTrimGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(qualityTrimGrid, 2);
            downloadOptionsGrid.Children.Add(qualityTrimGrid);

            // Quality grid
            Grid qualityGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            qualityGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            qualityGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            qualityGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(qualityGrid, 0);
            qualityTrimGrid.Children.Add(qualityGrid);

            // Quality textblock
            TextBlock AddPlaylistVideoDownloadOptionsQualityTextBlock = new TextBlock
            {
                Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoDownloadOptionsQualityTextBlock"),
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsQualityTextBlock, 0);
            qualityGrid.Children.Add(AddPlaylistVideoDownloadOptionsQualityTextBlock);

            // Quality combobox
            ComboBox AddPlaylistVideoDownloadOptionsQualityComboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsQualityComboBox, 2);
            qualityGrid.Children.Add(AddPlaylistVideoDownloadOptionsQualityComboBox);

            // Trim grid
            Grid trimGrid = new Grid();
            trimGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            trimGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            trimGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(trimGrid, 2);
            qualityTrimGrid.Children.Add(trimGrid);

            // Trim textblock
            TextBlock AddPlaylistVideoDownloadOptionsTrimTextBlock = new TextBlock
            {
                Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoDownloadOptionsTrimTextBlock"),
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetRow(AddPlaylistVideoDownloadOptionsTrimTextBlock, 0);
            trimGrid.Children.Add(AddPlaylistVideoDownloadOptionsTrimTextBlock);

            // Trim textbox grid
            Grid trimTextBoxGrid = new Grid();
            trimTextBoxGrid.ColumnDefinitions.Add(new ColumnDefinition());
            trimTextBoxGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            trimTextBoxGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            trimTextBoxGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            trimTextBoxGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(trimTextBoxGrid, 2);
            trimGrid.Children.Add(trimTextBoxGrid);

            // Trim start textbox
            TextBox AddPlaylistVideoDownloadOptionsTrimStartTextBox = new TextBox();
            TextBoxExtensions.SetCustomMask(AddPlaylistVideoDownloadOptionsTrimStartTextBox, "5:[0-5]");
            TextBoxExtensions.SetMask(AddPlaylistVideoDownloadOptionsTrimStartTextBox, "99:59:59");
            Grid.SetColumn(AddPlaylistVideoDownloadOptionsTrimStartTextBox, 0);
            trimTextBoxGrid.Children.Add(AddPlaylistVideoDownloadOptionsTrimStartTextBox);

            // Trim separator
            TextBlock AddPlaylistVideoDownloadOptionsTrimSeparatorTextBlock = new TextBlock
            {
                Text = "-",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(AddPlaylistVideoDownloadOptionsTrimSeparatorTextBlock, 2);
            trimTextBoxGrid.Children.Add(AddPlaylistVideoDownloadOptionsTrimSeparatorTextBlock);

            // Trim end textbox
            TextBox AddPlaylistVideoDownloadOptionsTrimEndTextBox = new TextBox();
            TextBoxExtensions.SetCustomMask(AddPlaylistVideoDownloadOptionsTrimEndTextBox, "5:[0-5]");
            TextBoxExtensions.SetMask(AddPlaylistVideoDownloadOptionsTrimEndTextBox, "99:59:59");
            Grid.SetColumn(AddPlaylistVideoDownloadOptionsTrimEndTextBox, 4);
            trimTextBoxGrid.Children.Add(AddPlaylistVideoDownloadOptionsTrimEndTextBox);


            // File & location
            Grid fileLocationGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            fileLocationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            fileLocationGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(10) });
            fileLocationGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            fileLocationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            fileLocationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });
            fileLocationGrid.ColumnDefinitions.Add(new ColumnDefinition());
            Grid.SetRow(fileLocationGrid, 4);
            content.Children.Add(fileLocationGrid);

            // File textblock
            TextBlock AddPlaylistVideoFileDataTextBlock = new TextBlock
            {
                Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoFileDataTextBlock"),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(AddPlaylistVideoFileDataTextBlock, 0);
            Grid.SetColumn(AddPlaylistVideoFileDataTextBlock, 0);
            fileLocationGrid.Children.Add(AddPlaylistVideoFileDataTextBlock);

            // File grid
            Grid fileGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            fileGrid.ColumnDefinitions.Add(new ColumnDefinition());
            fileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });
            fileGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetRow(fileGrid, 0);
            Grid.SetColumn(fileGrid, 2);
            fileLocationGrid.Children.Add(fileGrid);

            // Filename textbox
            TextBox AddPlaylistVideoFileDataFilenameTextBox = new TextBox();
            Grid.SetColumn(AddPlaylistVideoFileDataFilenameTextBox, 0);
            fileGrid.Children.Add(AddPlaylistVideoFileDataFilenameTextBox);

            // Extension combobox
            ComboBox AddPlaylistVideoFileDataExtensionComboBox = new ComboBox
            {
                Width = 80
            };
            Grid.SetColumn(AddPlaylistVideoFileDataExtensionComboBox, 2);
            fileGrid.Children.Add(AddPlaylistVideoFileDataExtensionComboBox);

            // Location textblock
            TextBlock AddPlaylistVideoLocationDataTextBlock = new TextBlock
            {
                Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoLocationDataTextBlock"),
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetRow(AddPlaylistVideoLocationDataTextBlock, 2);
            Grid.SetColumn(AddPlaylistVideoLocationDataTextBlock, 0);
            fileLocationGrid.Children.Add(AddPlaylistVideoLocationDataTextBlock);

            // Location grid
            Grid locationGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            locationGrid.ColumnDefinitions.Add(new ColumnDefinition());
            locationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0) });
            locationGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetRow(locationGrid, 2);
            Grid.SetColumn(locationGrid, 2);
            fileLocationGrid.Children.Add(locationGrid);

            // Location data textblock
            TextBlock AddPlaylistVideoLocationDataLocationTextBlock = new TextBlock
            {
                Text = "//Location",
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(AddPlaylistVideoLocationDataLocationTextBlock, 0);
            locationGrid.Children.Add(AddPlaylistVideoLocationDataLocationTextBlock);

            // Location selection button
            Button AddPlaylistVideoLocationDataChooseLocationButton = new Button
            {
                Content = "...",
            };
            Grid.SetColumn(AddPlaylistVideoLocationDataChooseLocationButton, 2);
            locationGrid.Children.Add(AddPlaylistVideoLocationDataChooseLocationButton);



            // Set items in quality combobox
            foreach (string q in Video.Streams.Keys)
            {
                AddPlaylistVideoDownloadOptionsQualityComboBox.Items.Add(q);
            }

            // Set items in extension combobox
            foreach (string x in Video.MediaType == "A" ? Config.DefaultAudioExtensionList : Config.DefaultVideoExtensionList)
            {
                AddPlaylistVideoFileDataExtensionComboBox.Items.Add(x);
            }

            // Set quality option
            if (Video.MediaType == "A")
            {
                AddPlaylistVideoDownloadOptionsQualityComboBox.SelectedValue = ResourceLoader.GetForCurrentView().GetString("AddVideoQualityNoVideoStream");
                AddPlaylistVideoDownloadOptionsQualityComboBox.IsEnabled = false;
            }
            else
            {
                AddPlaylistVideoDownloadOptionsQualityComboBox.SelectedValue = Video.SelectedQuality;
                AddPlaylistVideoDownloadOptionsQualityComboBox.IsEnabled = true;
            }

            // Set trim options
            AddPlaylistVideoDownloadOptionsTrimStartTextBox.Text = Video.TrimStart.ToString();
            AddPlaylistVideoDownloadOptionsTrimEndTextBox.Text = Video.TrimEnd.ToString();

            // Set media type option
            switch (Video.MediaType)
            {
                case "AV": AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV.IsChecked = true; break;
                case "V": AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonV.IsChecked = true; break;
                case "A": AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonA.IsChecked = true; break;
                default: AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV.IsChecked = true; break;
            }
            
            // Set filename option
            AddPlaylistVideoFileDataFilenameTextBox.Text = Video.Filename;

            // Set extension option
            AddPlaylistVideoFileDataExtensionComboBox.SelectedItem = Video.Extension;

            // Set location option
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("save"))
            {
                Video.CustomSaveLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("save");
                AddPlaylistVideoLocationDataLocationTextBlock.Text = Video.CustomSaveLocation.Path;
                Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
            }
            else
            {
                AddPlaylistVideoLocationDataLocationTextBlock.Text = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\";
                Video.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Video.Filename}.{Video.Extension.ToLower()}";
            }

            // Set event handlers
            AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonAV.Checked += (object sender, RoutedEventArgs e) =>
            {
                Video.MediaType = "AV";
                AddPlaylistVideoDownloadOptionsQualityComboBox.IsEnabled = true;
                AddPlaylistVideoDownloadOptionsQualityComboBox.SelectedValue = Video.Streams.Keys.ToArray()[0];
                AddPlaylistVideoFileDataExtensionComboBox.Items.Clear();
                foreach (string x in Config.DefaultVideoExtensionList)
                {
                    AddPlaylistVideoFileDataExtensionComboBox.Items.Add(x);
                }
                AddPlaylistVideoFileDataExtensionComboBox.SelectedItem = Config.GetValue("default_video_extension");
            };
            AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonA.Checked += (object sender, RoutedEventArgs e) =>
            {
                Video.MediaType = "A";
                AddPlaylistVideoDownloadOptionsQualityComboBox.IsEnabled = false;
                AddPlaylistVideoDownloadOptionsQualityComboBox.SelectedValue = ResourceLoader.GetForCurrentView().GetString("AddPlaylistVideoQualityNoVideoStream");
                AddPlaylistVideoFileDataExtensionComboBox.Items.Clear();
                foreach (string x in Config.DefaultAudioExtensionList)
                {
                    AddPlaylistVideoFileDataExtensionComboBox.Items.Add(x);
                }
                AddPlaylistVideoFileDataExtensionComboBox.SelectedItem = Config.GetValue("default_audio_extension");
            };
            AddPlaylistVideoDownloadOptionsMediaTypeRadiobuttonV.Checked += (object sender, RoutedEventArgs e) =>
            {
                Video.MediaType = "V";
                AddPlaylistVideoDownloadOptionsQualityComboBox.IsEnabled = true;
                AddPlaylistVideoDownloadOptionsQualityComboBox.SelectedValue = Video.Streams.Keys.ToArray()[0];
                AddPlaylistVideoFileDataExtensionComboBox.Items.Clear();
                foreach (string x in Config.DefaultVideoExtensionList)
                {
                    AddPlaylistVideoFileDataExtensionComboBox.Items.Add(x);
                }
                AddPlaylistVideoFileDataExtensionComboBox.SelectedItem = Config.GetValue("default_video_extension");
            };
            AddPlaylistVideoDownloadOptionsQualityComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                if (Video.MediaType == "A")
                    Video.SelectedQuality = Video.Streams.Keys.ToArray()[0];
                else
                    Video.SelectedQuality = (string)AddPlaylistVideoDownloadOptionsQualityComboBox.SelectedItem;
            };
            AddPlaylistVideoDownloadOptionsTrimStartTextBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                try
                {
                    Video.TrimStart = TimeSpan.Parse(AddPlaylistVideoDownloadOptionsTrimStartTextBox.Text);
                }
                catch { }
            };
            AddPlaylistVideoDownloadOptionsTrimEndTextBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                try
                {
                    Video.TrimEnd = TimeSpan.Parse(AddPlaylistVideoDownloadOptionsTrimEndTextBox.Text);
                }
                catch { }
            };
            AddPlaylistVideoFileDataFilenameTextBox.TextChanged += (object sender, TextChangedEventArgs e) =>
            {
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    AddPlaylistVideoFileDataFilenameTextBox.Text = AddPlaylistVideoFileDataFilenameTextBox.Text.Replace(c, ' ');
                }
                Video.Filename = AddPlaylistVideoFileDataFilenameTextBox.Text;
                if (Video.CustomSaveLocation != null)
                    Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
                else
                    Video.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Video.Filename}.{Video.Extension.ToLower()}";
            };
            AddPlaylistVideoFileDataExtensionComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                if (!(AddPlaylistVideoFileDataExtensionComboBox.SelectedItem == null))
                {
                    Video.Extension = (string)AddPlaylistVideoFileDataExtensionComboBox.SelectedItem;
                    if (Video.CustomSaveLocation != null)
                        Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
                    else
                        Video.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{Video.Filename}.{Video.Extension.ToLower()}";
                }
            };
            AddPlaylistVideoLocationDataChooseLocationButton.Click += async (object sender, RoutedEventArgs e) =>
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
                        AddPlaylistVideoLocationDataLocationTextBlock.Text = Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\';
                        Video.FilePath = $@"{(Video.CustomSaveLocation.Path[Video.CustomSaveLocation.Path.Length - 1] == '\\' ? Video.CustomSaveLocation.Path : Video.CustomSaveLocation.Path + '\\')}{Video.Filename}.{Video.Extension.ToLower()}";
                    }
                    catch { }
                }
            };



            // Add panel
            VObjects[Video] = AddPlaylistVideoLocationDataLocationTextBlock;
            PlaylistPanel.Children.Add(videoPanel);
        }

        // DELETED VIDEOS PANEL HANDLER
        private void DeletedVideosPanelHandler()
        {
            if (DeletedVideosPanel == null)
            {
                // Init panel
                DeletedVideosPanel = new Grid
                {
                    Background = new SolidColorBrush((Color)Application.Current.Resources["SystemChromeMediumHighColor"]),
                    BorderThickness = new Thickness(10),
                    BorderBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemChromeMediumHighColor"]),
                    CornerRadius = new CornerRadius(1),
                    Margin = new Thickness(0, 5, 0, 5),
                };
                DeletedVideosPanel.ColumnDefinitions.Add(new ColumnDefinition());
                DeletedVideosPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
                DeletedVideosPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Textblock
                TextBlock AddPlaylistDeletedVideosPanelTextBlock = new TextBlock
                {
                    Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistDeletedVideosPanelTextBlock").Replace("{x}", DeletedVObjects.Count.ToString()),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                Grid.SetColumn(AddPlaylistDeletedVideosPanelTextBlock, 0);
                DeletedVideosPanel.Children.Add(AddPlaylistDeletedVideosPanelTextBlock);

                // Button
                Button AddPlaylistDeletedVideosPanelButton = new Button
                {
                    Content = ResourceLoader.GetForCurrentView().GetString("AddPlaylistDeletedVideosPanelButton")
                };
                AddPlaylistDeletedVideosPanelButton.Click += async (object sender, RoutedEventArgs e) =>
                {
                    foreach (VObject v in DeletedVObjects)
                    {
                        await VideoPanelHandler(v);
                    }
                    DeletedVObjects.Clear();
                    PlaylistPanel.Children.Remove(DeletedVideosPanel);
                    DeletedVideosPanel = null;
                };
                Grid.SetColumn(AddPlaylistDeletedVideosPanelButton, 2);
                DeletedVideosPanel.Children.Add(AddPlaylistDeletedVideosPanelButton);

                // Add panel
                PlaylistPanel.Children.Add(DeletedVideosPanel);
            }
            else
            {
                // Update panel
                ((TextBlock)DeletedVideosPanel.Children[0]).Text = ResourceLoader.GetForCurrentView().GetString("AddPlaylistDeletedVideosPanelTextBlock").Replace("{x}", DeletedVObjects.Count.ToString());
            }
        }

        #endregion
    }
}
