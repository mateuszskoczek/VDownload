using System;
using VDownload.Core.Enums;
using Windows.Storage;

namespace VDownload.Core.Structs
{
    public struct TaskOptions
    {
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
