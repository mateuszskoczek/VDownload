using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Sources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.AddPlaylist
{
    public sealed partial class AddPlaylistBase : ContentDialog
    {
        public AddPlaylistBase()
        {
            this.InitializeComponent();
            AddPlaylistContent.Navigate(typeof(AddPlaylistStart));
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Attach video info as content
            Content = AddPlaylistMain.Playlist.VObjects.Keys.ToArray();
        }

        private async void AddPlaylistSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to loading page
            AddPlaylistContent.Navigate(typeof(AddPlaylistLoading));
            IsPrimaryButtonEnabled = false;

            // Check url and get data
            Uri url = null;
            try
            {
                // Get url from textbox
                url = new Uri(AddPlaylistUrlTextBox.Text);
            }
            catch
            {
                // Navigate to not found page if url or path is invalid
                AddPlaylistContent.Navigate(typeof(AddPlaylistNotFound));
            }
            finally
            {
                if (url != null)
                {
                    try
                    {
                        // Get videos list
                        PObject playlist = new PObject(url);
                        await playlist.GetVideos();

                        if (playlist.VObjects.Count > 0)
                        {
                            // Navigate to playlist main page
                            AddPlaylistContent.Navigate(typeof(AddPlaylistMain), playlist);
                            IsPrimaryButtonEnabled = true;
                        }
                        else
                        {
                            // Navigate to not found page if playlist is empty
                            AddPlaylistContent.Navigate(typeof(AddPlaylistNotFound));
                        }
                    }
                    catch
                    {
                        // Navigate to not found page if url or path is invalid
                        AddPlaylistContent.Navigate(typeof(AddPlaylistNotFound));
                    }
                }
                else
                {
                    // Navigate to not found page if url or path is invalid
                    AddPlaylistContent.Navigate(typeof(AddPlaylistNotFound));
                }
            }
        }
    }
}
