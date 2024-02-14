using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Tasks
{
    public enum DownloadTaskStatus
    {
        Idle,
        EndedSuccessfully,
        EndedUnsuccessfully,
        EndedCancelled,
        Queued,
        Initializing,
        Finalizing,
        Processing,
        Downloading,
    }
}
