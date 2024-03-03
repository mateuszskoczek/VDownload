using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.StoragePicker;
using VDownload.Services.Utility.Filename;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomeVideoViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IDownloadTaskManager _tasksManager;

        protected readonly ISettingsService _settingsService;
        protected readonly IStoragePickerService _storagePickerService;
        protected readonly IFilenameService _filenameService;

        #endregion



        #region FIELDS

        protected Video _video;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected Uri _thumbnailUrl;

        [ObservableProperty]
        protected string _title;

        [ObservableProperty]
        protected string _author;

        [ObservableProperty]
        protected DateTime _publishDate;

        [ObservableProperty]
        protected TimeSpan _duration;

        [ObservableProperty]
        protected long _views;


        [ObservableProperty]
        protected ObservableCollection<VideoStream> _streams;

        [ObservableProperty]
        protected VideoStream _selectedStream;

        [ObservableProperty]
        protected MediaType _mediaType;

        [ObservableProperty]
        protected TimeSpan _trimStart;

        [ObservableProperty]
        protected TimeSpan _trimEnd;

        [ObservableProperty]
        protected string _directoryPath;

        [ObservableProperty]
        protected string _filename;

        [ObservableProperty]
        protected VideoExtension _videoExtension;

        [ObservableProperty]
        protected AudioExtension _audioExtension;

        #endregion



        #region CONSTRUCTORS

        public HomeVideoViewModel(IDownloadTaskManager tasksManager, ISettingsService settingsService, IStoragePickerService storagePickerService, IFilenameService filenameService) 
        {
            _tasksManager = tasksManager;
            _settingsService = settingsService;
            _storagePickerService = storagePickerService;
            _filenameService = filenameService;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task LoadVideo(Video video)
        {
            await _settingsService.Load();

            _video = video;

            ThumbnailUrl = video.ThumbnailUrl;
            Title = video.Title;
            Author = video.Author;
            PublishDate = video.PublishDate;
            Duration = video.Duration;
            Views = video.Views;

            Streams = [.. video.Streams];
            SelectedStream = Streams[0];
            MediaType = _settingsService.Data.Common.Tasks.DefaultMediaType;
            TrimStart = TimeSpan.Zero;
            TrimEnd = Duration;
            DirectoryPath = _settingsService.Data.Common.Tasks.DefaultOutputDirectory;
            Filename = _filenameService.CreateFilename(_settingsService.Data.Common.Tasks.FilenameTemplate, video);
            VideoExtension = _settingsService.Data.Common.Tasks.DefaultVideoExtension;
            AudioExtension = _settingsService.Data.Common.Tasks.DefaultAudioExtension;
        }


        [RelayCommand]
        public async Task Browse()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                this.DirectoryPath = newDirectory;
            }
        }

        [RelayCommand]
        public void CreateTask()
        {
            _tasksManager.AddTask(_video, BuildDownloadOptions());
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        [RelayCommand]
        public void CreateTaskAndDownload()
        {
            DownloadTask task = _tasksManager.AddTask(_video, BuildDownloadOptions());
            CloseRequested?.Invoke(this, EventArgs.Empty);
            task.Enqueue();
        }

        #endregion



        #region PRIVATE METHODS

        protected VideoDownloadOptions BuildDownloadOptions()
        {
            return new VideoDownloadOptions(Duration)
            {
                MediaType = this.MediaType,
                SelectedStream = this.SelectedStream,
                TrimStart = this.TrimStart,
                TrimEnd = this.TrimEnd,
                Directory = this.DirectoryPath,
                Filename = _filenameService.SanitizeFilename(this.Filename),
                Extension = (this.MediaType == MediaType.OnlyAudio ? this.AudioExtension.ToString() : this.VideoExtension.ToString()).ToLower(),
            };
        }

        #endregion



        #region EVENTS

        public event EventHandler CloseRequested;

        #endregion
    }
}
