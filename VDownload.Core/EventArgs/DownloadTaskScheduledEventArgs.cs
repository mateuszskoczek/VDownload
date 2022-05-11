using System;

namespace VDownload.Core.EventArgs
{
    public class DownloadTaskScheduledEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public DownloadTaskScheduledEventArgs(DateTime scheduledFor)
        {
            ScheduledFor = scheduledFor;
        }

        #endregion



        #region PROPERTIES

        public DateTime ScheduledFor { get; private set; }

        #endregion
    }
}
