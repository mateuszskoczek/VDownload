using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Sources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.Views.AddVideo
{
    public sealed partial class AddVideoBase : ContentDialog
    {
        public AddVideoBase()
        {
            this.InitializeComponent();
            AddVideoContent.Navigate(typeof(AddVideoStart));
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Attach video info as content
            Content = AddVideoMain.Video;
        }

        private async void AddVideoSearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to loading page
            AddVideoContent.Navigate(typeof(AddVideoLoading));
            IsPrimaryButtonEnabled = false;

            // Check url and get data
            Uri url = null;
            try
            {
                // Get url from textbox
                url = new Uri(AddVideoUrlTextBox.Text);
            }
            catch
            {
                // Navigate to not found page if url is invalid
                AddVideoContent.Navigate(typeof(AddVideoNotFound));
            }
            finally
            {
                if (url != null)
                {
                    try
                    {
                        // Get video data
                        VObject video = new VObject(url);
                        await video.GetMetadata();

                        // Navigate to video found page
                        AddVideoContent.Navigate(typeof(AddVideoMain), video);
                        IsPrimaryButtonEnabled = true;
                    }
                    catch
                    {
                        // Navigate to not found page if url is invalid
                        AddVideoContent.Navigate(typeof(AddVideoNotFound));
                    }
                }
                else
                {
                    // Navigate to not found page if url is invalid
                    AddVideoContent.Navigate(typeof(AddVideoNotFound));
                }
            }
        }
    }
}
