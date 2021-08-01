// System
using System;



namespace VDownload.Core.Exceptions
{
    class InvalidConfigKeyException : Exception
    {
        public InvalidConfigKeyException()
        {
        }

        public InvalidConfigKeyException(string message)
            : base(message)
        {
        }

        public InvalidConfigKeyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
