using System;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;
using Windows.Storage;

namespace VDownload.Core.Structs
{
    public struct TaskData
    {
        public IVideoService VideoService { get; set; }
        public TaskOptions TaskOptions { get; set; }
    }
}
