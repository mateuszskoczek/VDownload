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
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        public DateTime ScheduledFor { get; private set; }
        public double DownloadingProgress { get; private set; }
        public double ProcessingProgress { get; private set; }
        public TimeSpan ElapsedTime { get; private set; }
        public Exception Exception { get; private set; }

        #endregion



        #region METHODS

        public async Task Run(bool delayWhenOnMeteredConnection)
        {
            StatusChanged.Invoke(this, System.EventArgs.Empty);

            CancellationTokenSource = new CancellationTokenSource();

            if (Schedule > 0)
            {
                ScheduledFor = DateTime.Now.AddMinutes(Schedule);
                Status = DownloadTaskStatus.Scheduled;
                StatusChanged.Invoke(this, System.EventArgs.Empty);
                while (DateTime.Now < ScheduledFor && !CancellationTokenSource.Token.IsCancellationRequested) await Task.Delay(100);
            }

            Status = DownloadTaskStatus.Queued;
            StatusChanged.Invoke(this, System.EventArgs.Empty);
            await DownloadTasksCollectionManagement.WaitInQueue(delayWhenOnMeteredConnection, CancellationTokenSource.Token);

            if (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                DownloadingProgress = 0;
                Status = DownloadTaskStatus.Downloading;
                StatusChanged.Invoke(this, System.EventArgs.Empty);

                StorageFolder tempFolder;
                if ((bool)Config.GetValue("custom_temp_location") && StorageApplicationPermissions.FutureAccessList.ContainsItem("custom_temp_location"))
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
                    StatusChanged.Invoke(this, System.EventArgs.Empty);

                    StorageFile outputFile = await File.Create();

                    CancellationTokenSource.Token.ThrowIfCancellationRequested();
                    await tempOutputFile.MoveAndReplaceAsync(outputFile);

                    taskStopwatch.Stop();

                    ElapsedTime = taskStopwatch.Elapsed;
                    Status = DownloadTaskStatus.EndedSuccessfully;
                    StatusChanged.Invoke(this, System.EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    endedWithError = true;
                    Exception = ex;
                    Status = DownloadTaskStatus.EndedUnsuccessfully;
                    StatusChanged.Invoke(this, System.EventArgs.Empty);
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
                Exception = new OperationCanceledException(CancellationTokenSource.Token);
                Status = DownloadTaskStatus.EndedUnsuccessfully;
                StatusChanged.Invoke(this, System.EventArgs.Empty);
            }
        }

        private void DownloadingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DownloadingProgress = e.Progress;
            Status = DownloadTaskStatus.Downloading;
            StatusChanged.Invoke(this, System.EventArgs.Empty);
        }

        private void ProcessingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProcessingProgress = e.Progress;
            Status = DownloadTaskStatus.Processing;
            StatusChanged.Invoke(this, System.EventArgs.Empty);
        }

        #endregion



        #region EVENT

        public event EventHandler StatusChanged;

        #endregion
    }
}
