using Microsoft.UI.Xaml.Controls;
using VDownload.Core.ViewModels.Settings;

namespace VDownload.Core.Views.Settings
{
    public sealed partial class SettingsView : Page
    {
        #region CONSTRUCTORS

        public SettingsView(SettingsViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        #endregion
    }
}
