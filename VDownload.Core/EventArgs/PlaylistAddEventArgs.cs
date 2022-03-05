using System;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using VDownload.Core.Objects;
using Windows.Storage;

namespace VDownload.Core.EventArgs
{
    public class PlaylistAddEventArgs : System.EventArgs
    {
        public (
            IVideoService VideoService,
            MediaType MediaType,
            IBaseStream Stream,
            TimeSpan TrimStart,
            TimeSpan TrimEnd,
            string Filename,
            MediaFileExtension Extension,
            StorageFolder Location,
            double Schedule
            )[] Videos { get; set; }
    }
}
