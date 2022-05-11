using System;

namespace VDownload.Core.EventArgs
{
    public class DownloadTaskEndedSuccessfullyEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public DownloadTaskEndedSuccessfullyEventArgs(TimeSpan elapsedTime)
        {
            ElapsedTime = elapsedTime;
        }

        #endregion



        #region PROPERTIES

        public TimeSpan ElapsedTime { get; private set; }

        #endregion
    }
}
