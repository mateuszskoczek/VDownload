using Microsoft.UI.Xaml.Controls;
using VDownload.Core.ViewModels.Home;

namespace VDownload.Core.Views.Home
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
