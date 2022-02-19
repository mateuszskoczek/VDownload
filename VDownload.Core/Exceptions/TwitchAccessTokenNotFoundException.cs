using System;

namespace VDownload.Core.Exceptions
{
    public class TwitchAccessTokenNotFoundException : Exception
    {
        public TwitchAccessTokenNotFoundException() { }
        public TwitchAccessTokenNotFoundException(string message) : base(message) { }
        public TwitchAccessTokenNotFoundException(string message, Exception inner) :base(message, inner) { }
    }
}
