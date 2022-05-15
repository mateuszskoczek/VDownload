using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.EventArgs;
using VDownload.Core.Interfaces;
using VDownload.Core.Services;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace VDownload.Core.Structs
{
    public class DownloadTask
    {
        #region CONSTRUCTORS

        public DownloadTask(string id, IVideo video, MediaType mediaType, BaseStream selectedStream, TrimData trim, OutputFile file, double schedule)
        {
            Id = id;
            Video = video;
            MediaType = mediaType;
            SelectedStream = selectedStream;
            Trim = trim;
            File = file;
            Schedule = schedule;

            Status = DownloadTaskStatus.Idle;
            LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status);
            CancellationTokenSource = new CancellationTokenSource();
        }

        #endregion



        #region PROPERTIES

        public string Id { get; set; }
        public IVideo Video { get; set; }
        public MediaType MediaType { get; set; }
        public BaseStream SelectedStream { get; set; }
        public TrimData Trim { get; set; }
        public OutputFile File { get; set; }
        public double Schedule { get; set; }
        public DownloadTaskStatus Status { get; private set; }
        public DownloadTaskStatusChangedEventArgs LastStatusChangedEventArgs { get; private set; }
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        #endregion



        #region METHODS

        public async Task Run(bool delayWhenOnMeteredConnection)
        {
            StatusChanged.Invoke(this, new DownloadTaskStatusChangedEventArgs(Status));

            CancellationTokenSource = new CancellationTokenSource();

            if (Schedule > 0)
            {
                DateTime scheduleFor = DateTime.Now.AddMinutes(Schedule);
                Status = DownloadTaskStatus.Scheduled;
                LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, scheduleFor);
                StatusChanged.Invoke(this, LastStatusChangedEventArgs);
                while (DateTime.Now < scheduleFor && !CancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }
            }

            Status = DownloadTaskStatus.Queued;
            LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status);
            StatusChanged.Invoke(this, LastStatusChangedEventArgs);
            await DownloadTasksCollectionManagement.WaitInQueue(delayWhenOnMeteredConnection, CancellationTokenSource.Token);

            if (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                Status = DownloadTaskStatus.Downloading;
                LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, 0);
                StatusChanged.Invoke(this, LastStatusChangedEventArgs);

                StorageFolder tempFolder;
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_temp_location"))
                {
                    tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("custom_temp_location");
                }
                else
                {
                    tempFolder = ApplicationData.Current.TemporaryFolder;
                }
                tempFolder = await tempFolder.CreateFolderAsync(Id);

                bool endedWithError = false;

                try
                {
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Stopwatch taskStopwatch = Stopwatch.StartNew();

                    ExtendedExecutionSession session = new ExtendedExecutionSession { Reason = ExtendedExecutionReason.Unspecified };
                    await session.RequestExtensionAsync();
                    CancellationTokenSource.Token.ThrowIfCancellationRequested();

                    Video.DownloadingProgressChanged += DownloadingProgressChanged;
                    Video.ProcessingProgressChanged += ProcessingProgressChanged;
                    StorageFile tempOutputFile = await Video.DownloadAndTranscodeAsync(tempFolder, SelectedStream, File.Extension, MediaType, Trim, CancellationTokenSource.Token);

                    session.Dispose();

                    Status = DownloadTaskStatus.Finalizing;
                    LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status);
                    StatusChanged.Invoke(this, LastStatusChangedEventArgs);

                    StorageFile outputFile = await File.Create();

                    CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await tempOutputFile.MoveAndReplaceAsync(outputFile);

                    taskStopwatch.Stop();

                    Status = DownloadTaskStatus.EndedSuccessfully;
                    LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, taskStopwatch.Elapsed);
                    StatusChanged.Invoke(this, LastStatusChangedEventArgs);
                }
                catch (Exception ex)
                {
                    endedWithError = true;
                    Status = DownloadTaskStatus.EndedUnsuccessfully;
                    LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, ex);
                    StatusChanged.Invoke(this, LastStatusChangedEventArgs);
                }
                finally
                {
                    if (!endedWithError || (bool)Config.GetValue("delete_task_temp_when_ended_with_error"))
                    {
                        // Delete temporary files
                        await tempFolder.DeleteAsync();
                    }
                }
            }
            else
            {
                Status = DownloadTaskStatus.EndedUnsuccessfully;
                LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, new OperationCanceledException(CancellationTokenSource.Token));
                StatusChanged.Invoke(this, LastStatusChangedEventArgs);
            }
        }

        private void DownloadingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Status = DownloadTaskStatus.Downloading;
            LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, e.Progress);
            StatusChanged.Invoke(this, LastStatusChangedEventArgs);
        }

        private void ProcessingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Status = DownloadTaskStatus.Processing;
            LastStatusChangedEventArgs = new DownloadTaskStatusChangedEventArgs(Status, e.Progress);
            StatusChanged.Invoke(this, LastStatusChangedEventArgs);
        }

        #endregion



        #region EVENT

        public event EventHandler<DownloadTaskStatusChangedEventArgs> StatusChanged;

        #endregion
    }
}
