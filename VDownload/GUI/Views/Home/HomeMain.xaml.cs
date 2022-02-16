using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.GUI.Views.Home
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeMain : Page
    {
        public HomeMain()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            IPlaylistService videoServices = new Core.Services.Sources.Twitch.Channel("jacexdowozwideo");
            await videoServices.GetMetadataAsync();
            Stopwatch sw = Stopwatch.StartNew();
            await videoServices.GetVideosAsync(500);
            sw.Stop();
            Debug.WriteLine(((Core.Services.Sources.Twitch.Channel)videoServices).Videos.Length);
            Debug.WriteLine(sw.Elapsed.TotalSeconds);

        }
    }
}
