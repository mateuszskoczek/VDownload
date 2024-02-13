using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Common
{
    public enum DownloadTaskStatus
    {
        Idle,
        EndedSuccessfully,
        EndedUnsuccessfully,
        EndedCancelled,
        Queued,
        Initializing,
        Processing,
        Downloading,
        Finalizing,
    }
}
