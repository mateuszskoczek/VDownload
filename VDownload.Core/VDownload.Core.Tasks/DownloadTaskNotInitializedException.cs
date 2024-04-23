using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Tasks
{
    public class DownloadTaskNotInitializedException : Exception
    {
        #region CONSTRUCTORS

        public DownloadTaskNotInitializedException() : base() { }

        public DownloadTaskNotInitializedException(string message) : base(message) { }

        public DownloadTaskNotInitializedException(string message, Exception inner) : base(message, inner) { }

        #endregion
    }
}
