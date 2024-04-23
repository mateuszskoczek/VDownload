using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Activation
{
    public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
    {
        #region PRIVATE METHODS

        protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            await Task.CompletedTask;
        }

        #endregion
    }
}
