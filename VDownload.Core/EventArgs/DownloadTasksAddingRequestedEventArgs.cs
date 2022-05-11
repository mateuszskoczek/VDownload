using VDownload.Core.Enums;
using VDownload.Core.Structs;

namespace VDownload.Core.EventArgs
{
    public class DownloadTasksAddingRequestedEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public DownloadTasksAddingRequestedEventArgs(DownloadTask[] downloadTasks, DownloadTasksAddingRequestSource requestSource)
        {
            DownloadTasks = downloadTasks;
            RequestSource = requestSource;
        }

        #endregion



        #region PROPERTIES

        public DownloadTask[] DownloadTasks { get; private set; }
        public DownloadTasksAddingRequestSource RequestSource { get; private set; }

        #endregion
    }
}
