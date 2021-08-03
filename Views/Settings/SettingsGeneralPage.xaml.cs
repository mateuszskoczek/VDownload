// Internal
using VDownload.Core.Services;

// System
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.Diagnostics;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsGeneralPage : Page
    {
        public SettingsGeneralPage()
        {
            this.InitializeComponent();

            // Set values
            MaxDownloadedVideosNumberBox.Value = int.Parse(Config.GetValue("max_downloaded_videos").ToString());
            MaxDownloadedChunksNumberBox.Value = int.Parse(Config.GetValue("max_downloaded_chunks").ToString());
        }
    }
}
