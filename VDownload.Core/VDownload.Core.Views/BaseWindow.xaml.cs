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
using VDownload.Core.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VDownload.Core.Views
{
    public sealed partial class BaseWindow : Window
    {
        #region PROPERTIES

        public XamlRoot XamlRoot => this.Root.XamlRoot;

        #endregion



        #region EVENTS

        public event EventHandler RootLoaded;

        #endregion



        #region CONSTRUCTORS

        public BaseWindow(BaseViewModel viewModel)
        {
            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(this.AppTitleBar);
            
            this.Root.DataContext = viewModel;
        }

        #endregion



        #region PRIVATE METHODS

        private void Root_Loaded(object sender, RoutedEventArgs e) => RootLoaded?.Invoke(this, EventArgs.Empty);

        #endregion
    }
}
