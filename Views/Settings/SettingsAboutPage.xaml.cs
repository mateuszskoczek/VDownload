using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VDownload.Views.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsAboutPage : Page
    {
        public SettingsAboutPage()
        {
            this.InitializeComponent();
        }

        private void ProgramName_Loaded(object sender, RoutedEventArgs e)
        {
            ProgramName.Text = Core.Global.ProgramInfo.Name;
        }

        private void ProgramVersion_Loaded(object sender, RoutedEventArgs e)
        {
            ProgramVersion.Text = $"{Core.Global.ProgramInfo.Version} ({Core.Global.ProgramInfo.Build})";
        }

        private static string VersionIconDark = "ms-appx:///Assets/Settings/About/VersionD.png";
        private static string VersionIconLight = "ms-appx:///Assets/Settings/About/VersionL.png";
        private void VersionIcon_Loaded(object sender, RoutedEventArgs e)
        {
            string imgFile;
            if (RequestedTheme == ElementTheme.Default)
            {
                var color = new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (color == "#FFFFFFFF")
                {
                    imgFile = VersionIconDark;
                }
                else
                {
                    imgFile = VersionIconLight;
                }
            }
            else if (RequestedTheme == ElementTheme.Dark)
            {
                imgFile = VersionIconLight;
            }
            else
            {
                imgFile = VersionIconDark;
            }
            var img = new BitmapImage();
            var uri = new Uri(imgFile);
            img.UriSource = uri;
            VersionIcon.Source = img;
        }

        private void ProgramAuthor_Loaded(object sender, RoutedEventArgs e)
        {
            ProgramAuthor.Text = Core.Global.ProgramInfo.Author;
        }

        private static string AuthorIconDark = "ms-appx:///Assets/Settings/About/AuthorD.png";
        private static string AuthorIconLight = "ms-appx:///Assets/Settings/About/AuthorL.png";
        private void AuthorIcon_Loaded(object sender, RoutedEventArgs e)
        {
            string imgFile;
            if (RequestedTheme == ElementTheme.Default)
            {
                var color = new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (color == "#FFFFFFFF")
                {
                    imgFile = AuthorIconDark;
                }
                else
                {
                    imgFile = AuthorIconLight;
                }
            }
            else if (RequestedTheme == ElementTheme.Dark)
            {
                imgFile = AuthorIconLight;
            }
            else
            {
                imgFile = AuthorIconDark;
            }
            var img = new BitmapImage();
            var uri = new Uri(imgFile);
            img.UriSource = uri;
            AuthorIcon.Source = img;
        }

        private async void ProgramAuthorGithub_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Core.Global.ProgramInfo.AuthorGithub);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private static string CopyrightIconDark = "ms-appx:///Assets/Settings/About/CopyrightD.png";
        private static string CopyrightIconLight = "ms-appx:///Assets/Settings/About/CopyrightL.png";
        private void CopyrightIcon_Loaded(object sender, RoutedEventArgs e)
        {
            string imgFile;
            if (RequestedTheme == ElementTheme.Default)
            {
                var color = new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (color == "#FFFFFFFF")
                {
                    imgFile = CopyrightIconDark;
                }
                else
                {
                    imgFile = CopyrightIconLight;
                }
            }
            else if (RequestedTheme == ElementTheme.Dark)
            {
                imgFile = CopyrightIconLight;
            }
            else
            {
                imgFile = CopyrightIconDark;
            }
            var img = new BitmapImage();
            var uri = new Uri(imgFile);
            img.UriSource = uri;
            CopyrightIcon.Source = img;
        }

        private void CopyrightTime_Loaded(object sender, RoutedEventArgs e)
        {
            CopyrightTime.Text = $"{Core.Global.ProgramInfo.CopyrightSince} - {Core.Global.ProgramInfo.CopyrightTo}";
        }

        private static string RepositoryIconDark = "ms-appx:///Assets/Settings/About/RepositoryD.png";
        private static string RepositoryIconLight = "ms-appx:///Assets/Settings/About/RepositoryL.png";
        private void RepositoryIcon_Loaded(object sender, RoutedEventArgs e)
        {
            string imgFile;
            if (RequestedTheme == ElementTheme.Default)
            {
                var color = new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (color == "#FFFFFFFF")
                {
                    imgFile = RepositoryIconDark;
                }
                else
                {
                    imgFile = RepositoryIconLight;
                }
            }
            else if (RequestedTheme == ElementTheme.Dark)
            {
                imgFile = RepositoryIconLight;
            }
            else
            {
                imgFile = RepositoryIconDark;
            }
            var img = new BitmapImage();
            var uri = new Uri(imgFile);
            img.UriSource = uri;
            RepositoryIcon.Source = img;
        }

        private async void RepositoryButtom_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Core.Global.ProgramInfo.Repository);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private static string DonationIconDark = "ms-appx:///Assets/Settings/About/DonationD.png";
        private static string DonationIconLight = "ms-appx:///Assets/Settings/About/DonationL.png";
        private void DonationIcon_Loaded(object sender, RoutedEventArgs e)
        {
            string imgFile;
            if (RequestedTheme == ElementTheme.Default)
            {
                var color = new Windows.UI.ViewManagement.UISettings().GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (color == "#FFFFFFFF")
                {
                    imgFile = DonationIconDark;
                }
                else
                {
                    imgFile = DonationIconLight;
                }
            }
            else if (RequestedTheme == ElementTheme.Dark)
            {
                imgFile = DonationIconLight;
            }
            else
            {
                imgFile = DonationIconDark;
            }
            var img = new BitmapImage();
            var uri = new Uri(imgFile);
            img.UriSource = uri;
            DonationIcon.Source = img;
        }

        private async void DonationButtom_Click(object sender, RoutedEventArgs e)
        {
            var uri = new Uri(Core.Global.ProgramInfo.Donation);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
