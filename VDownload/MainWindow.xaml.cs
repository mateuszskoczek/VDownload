using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
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
using VDownload.GUI.Services.Dialog;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.WindowManagement;
using WinRT.Interop;

namespace VDownload
{
    public sealed partial class MainWindow : Window
    {
        #region SERVICES

        private IDialogService _dialogService;

        #endregion



        #region CONSTRUCTORS

        public MainWindow(MainWindowViewModel viewModel, IDialogService dialogService)
        {
            _dialogService = dialogService;

            this.InitializeComponent();
            RootPanel.DataContext = viewModel;

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        #endregion



        #region EVENT HANDLER

        private void RootPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _dialogService.DefaultRoot = RootPanel.XamlRoot;
        }

        #endregion
    }
}
