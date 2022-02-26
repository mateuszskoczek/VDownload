using System;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using VDownload.Core.Objects;
using Windows.Storage;

namespace VDownload.Core.EventArgsObjects
{
    public class VideoAddEventArgs : EventArgs
    {
        public IVideoService VideoService { get; set; }
        public MediaType MediaType { get; set; }
        public Stream Stream { get; set; }
        public TimeSpan TrimStart { get; set; }
        public TimeSpan TrimEnd { get; set; }
        public string Filename { get; set; }
        public MediaFileExtension Extension { get; set; }
        public StorageFolder Location { get; set; }
    }
}
