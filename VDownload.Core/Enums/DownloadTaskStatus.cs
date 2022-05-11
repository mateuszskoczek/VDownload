namespace VDownload.Core.Enums
{
    public enum DownloadTaskStatus
    {
        Idle,
        Scheduled,
        Queued,
        Downloading,
        Processing,
        Finalizing,
        EndedSuccessfully,
        EndedUnsuccessfully,
    }
}
