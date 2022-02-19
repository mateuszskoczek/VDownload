using System;

namespace VDownload.Core.Exceptions
{
    public class TwitchAccessTokenNotValidException : Exception
    {
        public TwitchAccessTokenNotValidException() { }
        public TwitchAccessTokenNotValidException(string message) : base(message) { }
        public TwitchAccessTokenNotValidException(string message, Exception inner) : base(message, inner) { }
    }
}
