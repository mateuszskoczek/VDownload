using System;
using VDownload.Core.Enums;

namespace VDownload.Core.Interfaces
{
    public interface IVStream
    {
        #region PARAMETERS

        Uri Url { get; }
        bool IsChunked { get; }
        StreamType StreamType { get; }
        int Width { get; }
        int Height { get; }
        int FrameRate { get; }
        string VideoCodec { get; }

        #endregion
    }
}
