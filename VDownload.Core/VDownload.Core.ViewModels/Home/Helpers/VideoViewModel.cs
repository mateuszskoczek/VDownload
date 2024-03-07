using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Application;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.StoragePicker;
using VDownload.Services.Utility.Filename;

namespace VDownload.Core.ViewModels.Home.Helpers
{
    public partial class VideoViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly ISettingsService _settingsService;
        protected readonly IStoragePickerService _storagePickerService;
        protected readonly IFilenameService _filenameService;
        protected readonly IApplicationDataService _applicationDataService;

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

        public Video Video { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public VideoViewModel(Video video, ISettingsService settingsService, IStoragePickerService storagePickerService, IFilenameService filenameService, IApplicationDataService applicationDataService)
        {
            _settingsService = settingsService;
            _storagePickerService = storagePickerService;
            _filenameService = filenameService;
            _applicationDataService = applicationDataService;

            Video = video;

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
            Filename = _filenameService.CreateFilename(_settingsService.Data.Common.Tasks.FilenameTemplate, video);
            VideoExtension = _settingsService.Data.Common.Tasks.DefaultVideoExtension;
            AudioExtension = _settingsService.Data.Common.Tasks.DefaultAudioExtension;

            if (_settingsService.Data.Common.Tasks.SaveLastOutputDirectory && !string.IsNullOrWhiteSpace(_applicationDataService.Data.Common.LastOutputDirectory))
            {
                DirectoryPath = _applicationDataService.Data.Common.LastOutputDirectory;
            }
            else
            {
                DirectoryPath = _settingsService.Data.Common.Tasks.DefaultOutputDirectory;
            }
        }

        #endregion



        #region PUBLIC METHODS

        public VideoDownloadOptions BuildDownloadOptions()
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



        #region COMMANDS

        [RelayCommand]
        public async Task Browse()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                this.DirectoryPath = newDirectory;

                _applicationDataService.Data.Common.LastOutputDirectory = newDirectory;
                await _applicationDataService.Save();
            }
        }

        #endregion
    }
}
