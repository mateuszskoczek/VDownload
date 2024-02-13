using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Common;
using VDownload.Common.Models;

namespace VDownload.Tasks
{
    public partial class DownloadTask : ObservableObject
    {
        #region FIELDS

        private CancellationTokenSource? _cancellationTokenSource;

        private Task _downloadTask;

        private DispatcherQueue _dispatcherQueue;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private Video _video;

        [ObservableProperty]
        private VideoStream _videoStream;

        [ObservableProperty]
        private MediaType _mediaType;

        [ObservableProperty]
        private TimeSpan _trimStart;

        [ObservableProperty]
        private TimeSpan _trimEnd;

        [ObservableProperty]
        private string _directoryPath;

        [ObservableProperty]
        private string _filename;

        [ObservableProperty]
        private VideoExtension _videoExtension;

        [ObservableProperty]
        private AudioExtension _audioExtension;

        [ObservableProperty]
        private DownloadTaskStatus _status;

        [ObservableProperty]
        private DateTime _createDate;

        [ObservableProperty]
        private double _progress;

        public TimeSpan DurationAfterTrim
        {
            get => _durationAfterTrim;
            protected set => SetProperty(ref _durationAfterTrim, value);
        }
        protected TimeSpan _durationAfterTrim;

        public bool IsTrimmed
        {
            get => _isTrimmed;
            protected set => SetProperty(ref _isTrimmed, value);
        }
        protected bool _isTrimmed;

        public string FilePath
        {
            get => _filePath;
            protected set => SetProperty(ref _filePath, value);
        }
        protected string _filePath;

        #endregion



        #region CONSTRUCTORS

        public DownloadTask(Video video)
        {
            base.PropertyChanged += PropertyChanged;

            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            _video = video;
            _videoStream = _video.Streams[0];
            _mediaType = MediaType.Original; // TODO: get from settings
            _trimStart = TimeSpan.Zero;
            _trimEnd = _video.Duration;
            _directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); // TODO: get from settings
            _filename = _video.Title.Length > 50 ? _video.Title.Substring(0, 50) : _video.Title;
            _videoExtension = VideoExtension.MP4; // TODO: get from settings
            _audioExtension = AudioExtension.MP3; // TODO: get from settings
            _status = DownloadTaskStatus.Idle;
            _createDate = DateTime.Now;
            _progress = 0;
            _durationAfterTrim = _video.Duration;
            _isTrimmed = false;
            _filePath = $"{DirectoryPath}/{Filename}.{((object)(MediaType == MediaType.OnlyAudio ? AudioExtension : VideoExtension)).ToString().ToLower()}";
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

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            UpdateStatusWithDispatcher(DownloadTaskStatus.Initializing);

            _downloadTask = Task.Run(Download);
        }

        public void Cancel()
        {
            if (_cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }

        #endregion



        #region PRIVATE METHODS

        private void Download()
        {
            CancellationToken token = _cancellationTokenSource.Token;
        }

        private void UpdateStatusWithDispatcher(DownloadTaskStatus status)
        {
            _dispatcherQueue.TryEnqueue(() => Status = status);
        }

        #endregion



        #region EVENT HANDLERS

        private void PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Filename):
                case nameof(DirectoryPath):
                case nameof(VideoExtension):
                case nameof(AudioExtension):
                    FilePath = $"{DirectoryPath}/{Filename}.{((object)(MediaType == MediaType.OnlyAudio ? AudioExtension : VideoExtension)).ToString().ToLower()}";
                    break;
                case nameof(TrimStart):
                case nameof(TrimEnd): 
                    DurationAfterTrim = TrimEnd.Subtract(TrimStart);
                    break;
                case nameof(DurationAfterTrim):
                    IsTrimmed = DurationAfterTrim != Video.Duration;
                    break;
            }
        }

        #endregion
    }
}
