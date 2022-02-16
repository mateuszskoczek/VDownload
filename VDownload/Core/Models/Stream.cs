using System;
using VDownload.Core.Enums;
using VDownload.Core.Interfaces;

namespace VDownload.Core.Models
{
    public class Stream : IVStream, IAStream
    {
        #region PARAMETERS

        public Uri Url { get; private set; }
        public bool IsChunked { get; private set; }
        public StreamType StreamType { get; private set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int FrameRate { get; set; }
        public string VideoCodec { get; set; }
        public int AudioBitrate { get; set; }
        public string AudioCodec { get; set; }

        #endregion



        #region CONSTRUCTORS

        public Stream(Uri url, bool isChunked, StreamType streamType)
        {
            Url = url;
            IsChunked = isChunked;
            StreamType = streamType;
        }

        #endregion
    }
}
