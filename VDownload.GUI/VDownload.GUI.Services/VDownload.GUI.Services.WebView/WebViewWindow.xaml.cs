using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VDownload.GUI.Services.WebView
{
    public sealed partial class WebViewWindow : Window
    {
        #region FIEDLS

        private readonly Predicate<string> _defaultClosePredicate = args => false;

        private bool _isOpened;
        private Predicate<string> _closePredicate;

        #endregion



        #region CONSTRUCTORS

        public WebViewWindow(string name)
        {
            this.InitializeComponent();
            this.Title = name;

            _isOpened = false;
            _closePredicate = _defaultClosePredicate;
        }

        #endregion



        #region PUBLIC METHODS

        internal async Task<string> Show(Uri url, Predicate<string> closePredicate)
        {
            this.WebView.Source = url;
            _closePredicate = closePredicate;

            this.Activate();
            _isOpened = true;
            while (_isOpened)
            {
                await Task.Delay(10);
            }

            _closePredicate = _defaultClosePredicate;

            return this.WebView.Source.ToString();
        }

        #endregion



        #region EVENT HANDLER


        private void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (_closePredicate.Invoke(this.WebView.Source.ToString()))
            {
                this.Close();
            }
        }
        
        private void Window_Closed(object sender, WindowEventArgs args)
        {
            _isOpened = false;
        }

        #endregion
    }
}
