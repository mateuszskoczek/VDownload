using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Common.Exceptions
{
    public class MediaSearchException : Exception
    {
        #region CONSTRUCTORS

        public MediaSearchException() : base() { }

        public MediaSearchException(string message) : base(message) { }

        public MediaSearchException(string message, Exception inner) : base(message, inner) { }

        #endregion
    }
}
