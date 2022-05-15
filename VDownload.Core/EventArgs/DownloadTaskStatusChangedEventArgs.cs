using System;
using VDownload.Core.Enums;

namespace VDownload.Core.EventArgs
{
    public class DownloadTaskStatusChangedEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public DownloadTaskStatusChangedEventArgs(DownloadTaskStatus status)
        {
            Status = status;
        }

        public DownloadTaskStatusChangedEventArgs(DownloadTaskStatus status, DateTime scheduledFor)
        {
            Status = status;
            ScheduledFor = scheduledFor;
        }

        public DownloadTaskStatusChangedEventArgs(DownloadTaskStatus status, double progress)
        {
            Status = status;
            if (Status == DownloadTaskStatus.Downloading)
            {
                DownloadingProgress = progress;
            }
            else if (Status == DownloadTaskStatus.Processing)
            {
                ProcessingProgress = progress;
            }
        }

        public DownloadTaskStatusChangedEventArgs(DownloadTaskStatus status, TimeSpan elapsedTime)
        {
            Status = status;
            ElapsedTime = elapsedTime;
        }

        public DownloadTaskStatusChangedEventArgs(DownloadTaskStatus status, Exception exception)
        {
            Status = status;
            Exception = exception;
        }

        #endregion



        #region PROPERTIES

        public DownloadTaskStatus Status { get; private set; }
        public DateTime ScheduledFor { get; private set; }
        public double DownloadingProgress { get; private set; }
        public double ProcessingProgress { get; private set; }
        public TimeSpan ElapsedTime { get; private set; }
        public Exception Exception { get; private set; }

        #endregion
    }
}
