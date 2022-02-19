using System;
using VDownload.Core.Enums;

namespace VDownload.Core.Interfaces
{
    public interface IAStream
    {
        #region PROPERTIES

        Uri Url { get; }
        bool IsChunked { get; }
        StreamType StreamType { get; }
        int AudioBitrate { get; }
        string AudioCodec { get; }

        #endregion
    }
}
