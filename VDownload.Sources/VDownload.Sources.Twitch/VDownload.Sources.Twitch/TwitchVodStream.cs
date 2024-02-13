using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Common;

namespace VDownload.Sources.Twitch
{
    public partial class TwitchVodStream : VideoStream
    {
        #region PROPERTIES

        [ObservableProperty]
        private string _urlM3U8;

        [ObservableProperty]
        private int _height;

        [ObservableProperty]
        private int _width;

        [ObservableProperty]
        private string _codecs;

        #endregion
    }
}
