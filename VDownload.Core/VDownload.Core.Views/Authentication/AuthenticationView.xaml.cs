using Microsoft.UI.Xaml.Controls;
using VDownload.Core.ViewModels.Authentication;

namespace VDownload.Core.Views.Authentication
{
    public sealed partial class AuthenticationView : Page
    {
        #region CONSTRUCTORS

        public AuthenticationView(AuthenticationViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }

        #endregion
    }
}
