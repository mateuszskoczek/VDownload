using System;

namespace VDownload.Core.EventArgs
{
    public class DownloadTaskEndedUnsuccessfullyEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public DownloadTaskEndedUnsuccessfullyEventArgs(Exception exception)
        {
            Exception = exception;
        }

        #endregion



        #region PROPERTIES

        public Exception Exception { get; private set; }

        #endregion
    }
}
