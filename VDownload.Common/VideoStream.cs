using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Common
{
    public abstract partial class VideoStream : ObservableObject
    {
        #region PROPERTIES

        [ObservableProperty]
        protected string _streamIdentifier;

        [ObservableProperty]
        private int _width;

        #endregion
    }
}
