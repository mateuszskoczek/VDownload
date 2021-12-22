using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Sources;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.Views.AddPlaylist
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddPlaylistMain : Page
    {
        #region INIT

        // PLAYLIST OBJECT
        public static PObject Playlist;
        private StorageFolder ApplyToAllLocation;

        // CONSTRUCTOR
        public AddPlaylistMain()
        {
            InitializeComponent();
        }

        #endregion



        #region MAIN

        // NAVIGATED TO THIS PAGE
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set ApplyToAllLocation
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("save"))
            {
                ApplyToAllLocation = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("save");
                AddPlaylistLocationSelectionLocationTextBlock.Text = ApplyToAllLocation.Path;
            }
            else
            {
                AddPlaylistLocationSelectionLocationTextBlock.Text = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\";
            }

            // Get playlist object from parent
            base.OnNavigatedTo(e);
            Playlist = (PObject)e.Parameter;
            await Playlist.InitPlaylistPanel(AddPlaylistVideoPanel);

            
        }

        // SELECT LOCATION
        private async void AddPlaylistLocationSelectionSelectLocationButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            picker.FileTypeFilter.Add("*");
            StorageFolder folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                try
                {
                    await(await folder.CreateFileAsync("VDownloadLocationAccessTest")).DeleteAsync();
                    ApplyToAllLocation = folder;
                    AddPlaylistLocationSelectionLocationTextBlock.Text = ApplyToAllLocation.Path[ApplyToAllLocation.Path.Length - 1] == '\\' ? ApplyToAllLocation.Path : ApplyToAllLocation.Path + '\\';
                }
                catch { }
            }
        }

        // APPLY LOCATION
        private void AddPlaylistLocationSelectionApplyLocationButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<VObject, TextBlock> v in Playlist.VObjects)
            {
                if (ApplyToAllLocation != null)
                {
                    v.Key.CustomSaveLocation = ApplyToAllLocation;
                    v.Value.Text = v.Key.CustomSaveLocation.Path;
                    v.Key.FilePath = $@"{(v.Key.CustomSaveLocation.Path[v.Key.CustomSaveLocation.Path.Length - 1] == '\\' ? v.Key.CustomSaveLocation.Path : v.Key.CustomSaveLocation.Path + '\\')}{v.Key.Filename}.{v.Key.Extension.ToLower()}";
                }
                else
                {
                    v.Key.CustomSaveLocation = null;
                    v.Value.Text = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\";
                    v.Key.FilePath = $@"{UserDataPaths.GetDefault().Downloads}\VDownload\{v.Key.Filename}.{v.Key.Extension.ToLower()}";
                }
            }
        }

        #endregion
    }
}
