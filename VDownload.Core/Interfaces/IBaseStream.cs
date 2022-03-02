using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Enums;

namespace VDownload.Core.Interfaces
{
    public interface IBaseStream
    {
        #region PROPERTIES

        Uri Url { get; }
        bool IsChunked { get; }
        StreamType StreamType { get; }
        int Height { get; }
        int FrameRate { get; }

        #endregion
    }
}
