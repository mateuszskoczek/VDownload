using CommunityToolkit.Mvvm.ComponentModel;
using VDownload.Models;
using VDownload.Services.Utility.FFmpeg;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using Windows.System;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Windows.Storage;
using System.IO;
using VDownload.Services.UI.Notifications;
using VDownload.Services.UI.StringResources;
using System.Collections.Generic;

namespace VDownload.Core.Tasks
{
    public partial class DownloadTask : ObservableObject
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;
        protected readonly IFFmpegService _ffmpegService;
        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly INotificationsService _notificationsService;

        #endregion



        #region FIELDS

        private readonly DispatcherQueue _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        private CancellationTokenSource? _cancellationTokenSource;

        private Task _downloadTask;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected Guid _id;

        [ObservableProperty]
        protected Video _video;

        [ObservableProperty]
        protected VideoDownloadOptions _downloadOptions;

        [ObservableProperty]
        protected DownloadTaskStatus _status;

        [ObservableProperty]
        protected DateTime _createDate;

        [ObservableProperty]
        protected double _progress;

        [ObservableProperty]
        protected string? _error;

        #endregion



        #region CONSTRUCTORS

        internal DownloadTask(Video video, VideoDownloadOptions downloadOptions, IConfigurationService configurationService, ISettingsService settingsService, IFFmpegService ffmpegService, IStringResourcesService stringResourcesService, INotificationsService notificationsService)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;
            _ffmpegService = ffmpegService;
            _stringResourcesService = stringResourcesService;
            _notificationsService = notificationsService;

            _video = video;
            _downloadOptions = downloadOptions;
            _id = Guid.NewGuid();
            _status = DownloadTaskStatus.Idle;
            _createDate = DateTime.Now;
            _progress = 0;
        }

        #endregion



        #region PUBLIC METHODS

        public void Enqueue()
        {
            DownloadTaskStatus[] statuses =
            [
                DownloadTaskStatus.Idle,
                DownloadTaskStatus.EndedUnsuccessfully,
                DownloadTaskStatus.EndedSuccessfully,
                DownloadTaskStatus.EndedCancelled,
            ];
            if (statuses.Contains(Status))
            {
                Status = DownloadTaskStatus.Queued;
            }
        }

        internal void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            UpdateStatusWithDispatcher(DownloadTaskStatus.Initializing);

            _downloadTask = Download();
        }

        public async Task Cancel()
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                await _downloadTask;
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        #endregion



        #region PRIVATE METHODS

        private async Task Download()
        {
            CancellationToken token = _cancellationTokenSource.Token;

            await _settingsService.Load();

            string tempDirectory = $"{_settingsService.Data.Common.Temp.Directory}\\{_configurationService.Common.Path.Temp.TasksDirectory}\\{Id}";
            Directory.CreateDirectory(tempDirectory);

            List<string> content = new List<string>()
            {
                $"{_stringResourcesService.NotificationsResources.Get("Title")}: {Video.Title}",
                $"{_stringResourcesService.NotificationsResources.Get("Author")}: {Video.Author}"
            };

            try
            {
                IProgress<double> onProgressDownloading = new Progress<double>((value) =>
                {
                    UpdateStatusWithDispatcher(DownloadTaskStatus.Downloading);
                    UpdateProgressWithDispatcher(value);
                });

                VideoStreamDownloadResult downloadResult = await DownloadOptions.SelectedStream.Download(tempDirectory, onProgressDownloading, token, Video.Duration, DownloadOptions.TrimStart, DownloadOptions.TrimEnd);

                Action<double> onProgressProcessing = (value) =>
                {
                    UpdateStatusWithDispatcher(DownloadTaskStatus.Processing);
                    UpdateProgressWithDispatcher(value);
                };
                string outputPath = $"{tempDirectory}\\processed.{DownloadOptions.Extension}";

                FFmpegBuilder ffmpegBuilder = _ffmpegService.CreateBuilder();
                if (downloadResult.NewTrimStart != TimeSpan.Zero)
                {
                    ffmpegBuilder.TrimStart(downloadResult.NewTrimStart);
                }
                if (downloadResult.NewTrimEnd != downloadResult.NewDuration)
                {
                    ffmpegBuilder.TrimEnd(downloadResult.NewTrimEnd);
                }
                ffmpegBuilder.SetMediaType(DownloadOptions.MediaType)
                             .JoinCancellationToken(token)
                             .JoinProgressReporter(onProgressProcessing, downloadResult.NewDuration);
                await ffmpegBuilder.RunAsync(downloadResult.File, outputPath);

                UpdateStatusWithDispatcher(DownloadTaskStatus.Finalizing);

                string destination = $"{DownloadOptions.Directory}\\{DownloadOptions.Filename}.{DownloadOptions.Extension}";
                File.Copy(outputPath, destination, true);

                UpdateStatusWithDispatcher(DownloadTaskStatus.EndedSuccessfully);

                if (_settingsService.Data.Common.Notifications.OnSuccessful)
                {
                    string title = _stringResourcesService.NotificationsResources.Get("OnSuccessfulTitle");
                    _notificationsService.SendNotification(title, content);
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatusWithDispatcher(DownloadTaskStatus.EndedCancelled);
            }
            catch (Exception ex)
            {
                UpdateErrorWithDispatcher(ex.Message);
                UpdateStatusWithDispatcher(DownloadTaskStatus.EndedUnsuccessfully);

                if (_settingsService.Data.Common.Notifications.OnUnsuccessful)
                {
                    string title = _stringResourcesService.NotificationsResources.Get("OnSuccessfulTitle");
                    content.Add($"{_stringResourcesService.NotificationsResources.Get("Message")}: {ex.Message}");
                    _notificationsService.SendNotification(title, content);
                }
            }
            finally
            {
                if (Status != DownloadTaskStatus.EndedUnsuccessfully || _settingsService.Data.Common.Temp.DeleteOnError)
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        private void UpdateStatusWithDispatcher(DownloadTaskStatus status) => _dispatcherQueue.TryEnqueue(() => Status = status);

        private void UpdateProgressWithDispatcher(double progress) => _dispatcherQueue.TryEnqueue(() => Progress = progress);

        private void UpdateErrorWithDispatcher(string message) => _dispatcherQueue.TryEnqueue(() => Error = message);

        #endregion
    }
}