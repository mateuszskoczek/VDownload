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
using VDownload.Core.ViewModels.Subscriptions;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VDownload.Core.Views.Subscriptions
{
    public sealed partial class SubscriptionsView : Page
    {
        #region CONSTRUCTORS

        public SubscriptionsView(SubscriptionsViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        #endregion
    }
}
