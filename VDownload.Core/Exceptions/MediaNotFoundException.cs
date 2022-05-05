using System;

namespace VDownload.Core.Exceptions
{
    public class MediaNotFoundException : Exception
    {
        public MediaNotFoundException() { }
        public MediaNotFoundException(string message) : base(message) { }
        public MediaNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
