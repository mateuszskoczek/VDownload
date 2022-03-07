using System;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using Windows.Storage;

namespace VDownload.Core.Structs
{
    public struct TaskData
    {
        public IVideoService VideoService { get; set; }
        public MediaType MediaType { get; set; }
        public BaseStream Stream { get; set; }
        public TimeSpan TrimStart { get; set; }
        public TimeSpan TrimEnd { get; set; }
        public string Filename { get; set; }
        public MediaFileExtension Extension { get; set; }
        public StorageFolder Location { get; set; }
        public double Schedule { get; set; }
    }
}
