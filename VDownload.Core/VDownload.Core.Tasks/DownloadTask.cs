﻿using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Collections.Generic;
using System.Net.Http;
using Instances.Exceptions;
using FFMpegCore.Exceptions;
using VDownload.Core.Strings;

namespace VDownload.Core.Tasks
{
    public partial class DownloadTask : ObservableObject
    {
        #region ENUMS

        private enum TaskResult
        {
            Success,
            Cancellation,
            Error
        }

        #endregion



        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;
        protected readonly IFFmpegService _ffmpegService;
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

        internal DownloadTask(Video video, VideoDownloadOptions downloadOptions, IConfigurationService configurationService, ISettingsService settingsService, IFFmpegService ffmpegService, INotificationsService notificationsService)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;
            _ffmpegService = ffmpegService;
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

            string tempDirectory = $"{_settingsService.Data.Common.Temp.Directory}\\{_configurationService.Common.Path.Temp.TasksDirectory}\\{Guid.NewGuid()}";
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
            Directory.CreateDirectory(tempDirectory);

            List<string> content = new List<string>()
            {
                $"{StringResourcesManager.Notifications.Get("Title")}: {Video.Title}",
                $"{StringResourcesManager.Notifications.Get("Author")}: {Video.Author}"
            };

            string errorMessage = null;
            TaskResult? endingStatus = null;
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

                StorageFile outputFile = await StorageFile.GetFileFromPathAsync(outputPath);

                UpdateStatusWithDispatcher(DownloadTaskStatus.Finalizing);

                await Finalizing(outputFile);

                UpdateStatusWithDispatcher(DownloadTaskStatus.EndedSuccessfully);
                endingStatus = TaskResult.Success;
            }
            catch (OperationCanceledException)
            {
                UpdateStatusWithDispatcher(DownloadTaskStatus.EndedCancelled);
                endingStatus = TaskResult.Cancellation;
            }
            catch (Exception ex)
            {
                string message;

                if (ex is TaskCanceledException || ex is HttpRequestException)
                {
                    message = StringResourcesManager.HomeDownloadsView.Get("ErrorDownloadingTimeout");
                }
                else if (ex is InstanceFileNotFoundException)
                {
                    message = StringResourcesManager.HomeDownloadsView.Get("ErrorFFmpegPath");
                }
                else if (ex is FFMpegException)
                {
                    message = StringResourcesManager.HomeDownloadsView.Get("ErrorFFmpeg");
                }
                else
                {
                    message = ex.Message;
                }

                UpdateErrorWithDispatcher(message); 
                UpdateStatusWithDispatcher(DownloadTaskStatus.EndedUnsuccessfully);
                endingStatus = TaskResult.Error;
                errorMessage = message;
            }
            finally
            {
                switch (endingStatus)
                {
                    case TaskResult.Error:
                        if (_settingsService.Data.Common.Notifications.OnUnsuccessful)
                        {
                            string title = StringResourcesManager.Notifications.Get("OnUnsuccessfulTitle");
                            content.Add($"{StringResourcesManager.Notifications.Get("Message")}: {errorMessage}");
                            _notificationsService.SendNotification(title, content);
                        }
                        break;
                    case TaskResult.Success:
                        if (_settingsService.Data.Common.Notifications.OnSuccessful)
                        {
                            string title = StringResourcesManager.Notifications.Get("OnSuccessfulTitle");
                            _notificationsService.SendNotification(title, content);
                        }
                        break;
                }

                if (Status != DownloadTaskStatus.EndedUnsuccessfully || _settingsService.Data.Common.Temp.DeleteOnError)
                {
                    Directory.Delete(tempDirectory, true);
                }
            }
        }

        protected async Task Finalizing(StorageFile outputFile)
        {
            StorageFolder destination = await StorageFolder.GetFolderFromPathAsync(DownloadOptions.Directory);

            string filename = $"{DownloadOptions.Filename}.{DownloadOptions.Extension}";

            NameCollisionOption nameCollisionOption = _settingsService.Data.Common.Tasks.ReplaceOutputFileIfExists
                ? NameCollisionOption.ReplaceExisting
                : NameCollisionOption.GenerateUniqueName;

            await outputFile.CopyAsync(destination, filename, nameCollisionOption);
        }

        protected void UpdateStatusWithDispatcher(DownloadTaskStatus status) => _dispatcherQueue.TryEnqueue(() => Status = status);

        protected void UpdateProgressWithDispatcher(double progress) => _dispatcherQueue.TryEnqueue(() => Progress = progress);

        protected void UpdateErrorWithDispatcher(string message) => _dispatcherQueue.TryEnqueue(() => Error = message);

        #endregion
    }
}