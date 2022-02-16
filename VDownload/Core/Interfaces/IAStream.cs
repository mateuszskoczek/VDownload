using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Enums;

namespace VDownload.Core.Interfaces
{
    public interface IAStream
    {
        #region PARAMETERS

        Uri Url { get; }
        bool IsChunked { get; }
        StreamType StreamType { get; }
        int AudioBitrate { get; }
        string AudioCodec { get; }

        #endregion
    }
}
