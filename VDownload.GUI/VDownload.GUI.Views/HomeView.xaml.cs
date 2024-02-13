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
using VDownload.GUI.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VDownload.GUI.Views
{
    public sealed partial class HomeView : Page
    {
        #region CONSTRUCTORS

        public HomeView(HomeViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        #endregion
    }
}
