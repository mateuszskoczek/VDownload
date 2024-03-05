using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.Core.ViewModels.About;
using VDownload.Core.ViewModels.Home;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VDownload.Core.Views.About
{
    public sealed partial class AboutView : Page
    {
        #region CONSTRUCTORS

        public AboutView(AboutViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        #endregion
    }
}
