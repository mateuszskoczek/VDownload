﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Strings;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.StoragePicker;

namespace VDownload.Core.ViewModels.Settings
{
    public partial class SettingsViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly ISettingsService _settingsService;
        protected readonly IConfigurationService _configurationService;
        protected readonly IStoragePickerService _storagePickerService;

        #endregion



        #region PROPERTIES

        public int SearchingPlaylistCount
        {
            get => _settingsService.Data.Common.Searching.MaxNumberOfVideosToGetFromPlaylist;
            set => SetProperty(_settingsService.Data.Common.Searching.MaxNumberOfVideosToGetFromPlaylist, value, _settingsService.Data.Common.Searching, (u, n) => u.MaxNumberOfVideosToGetFromPlaylist = n);
        }

        public int TasksRunningTasks
        {
            get => _settingsService.Data.Common.Tasks.MaxNumberOfRunningTasks;
            set => SetProperty(_settingsService.Data.Common.Tasks.MaxNumberOfRunningTasks, value, _settingsService.Data.Common.Tasks, (u, n) => u.MaxNumberOfRunningTasks = n);
        }

        public MediaType TasksMediaType
        {
            get => _settingsService.Data.Common.Tasks.DefaultMediaType;
            set => SetProperty(_settingsService.Data.Common.Tasks.DefaultMediaType, value, _settingsService.Data.Common.Tasks, (u, n) => u.DefaultMediaType = n);
        }

        public VideoExtension TasksVideoExtension
        {
            get => _settingsService.Data.Common.Tasks.DefaultVideoExtension;
            set => SetProperty(_settingsService.Data.Common.Tasks.DefaultVideoExtension, value, _settingsService.Data.Common.Tasks, (u, n) => u.DefaultVideoExtension = n);
        }

        public AudioExtension TasksAudioExtension
        {
            get => _settingsService.Data.Common.Tasks.DefaultAudioExtension;
            set => SetProperty(_settingsService.Data.Common.Tasks.DefaultAudioExtension, value, _settingsService.Data.Common.Tasks, (u, n) => u.DefaultAudioExtension = n);
        }

        public string TasksFilenameTemplate
        {
            get => _settingsService.Data.Common.Tasks.FilenameTemplate;
            set => SetProperty(_settingsService.Data.Common.Tasks.FilenameTemplate, value, _settingsService.Data.Common.Tasks, (u, n) => u.FilenameTemplate = n);
        }

        public bool TasksMeteredConnectionWarning
        {
            get => _settingsService.Data.Common.Tasks.ShowMeteredConnectionWarnings;
            set => SetProperty(_settingsService.Data.Common.Tasks.ShowMeteredConnectionWarnings, value, _settingsService.Data.Common.Tasks, (u, n) => u.ShowMeteredConnectionWarnings = n);
        }

        public bool TasksSaveLastOutputDirectory
        {
            get => _settingsService.Data.Common.Tasks.SaveLastOutputDirectory;
            set => SetProperty(_settingsService.Data.Common.Tasks.SaveLastOutputDirectory, value, _settingsService.Data.Common.Tasks, (u, n) => u.SaveLastOutputDirectory = n);
        }

        public string TasksDefaultOutputDirectory
        {
            get => _settingsService.Data.Common.Tasks.DefaultOutputDirectory;
            set => SetProperty(_settingsService.Data.Common.Tasks.DefaultOutputDirectory, value, _settingsService.Data.Common.Tasks, (u, n) => u.DefaultOutputDirectory = n);
        }

        public bool TasksReplaceOutputFile
        {
            get => _settingsService.Data.Common.Tasks.ReplaceOutputFileIfExists;
            set => SetProperty(_settingsService.Data.Common.Tasks.ReplaceOutputFileIfExists, value, _settingsService.Data.Common.Tasks, (u, n) => u.ReplaceOutputFileIfExists = n);
        }

        public string ProcessingFFmpegLocation
        {
            get => _settingsService.Data.Common.Processing.FFmpegLocation;
            set => SetProperty(_settingsService.Data.Common.Processing.FFmpegLocation, value, _settingsService.Data.Common.Processing, (u, n) => u.FFmpegLocation = n);
        }

        public bool ProcessingUseHardwareAcceleration
        {
            get => _settingsService.Data.Common.Processing.UseHardwareAcceleration;
            set => SetProperty(_settingsService.Data.Common.Processing.UseHardwareAcceleration, value, _settingsService.Data.Common.Processing, (u, n) => u.UseHardwareAcceleration = n);
        }

        public bool ProcessingUseMultithreading
        {
            get => _settingsService.Data.Common.Processing.UseMultithreading;
            set => SetProperty(_settingsService.Data.Common.Processing.UseMultithreading, value, _settingsService.Data.Common.Processing, (u, n) => u.UseMultithreading = n);
        }

        public ProcessingSpeed ProcessingSpeed
        {
            get => _settingsService.Data.Common.Processing.Speed;
            set => SetProperty(_settingsService.Data.Common.Processing.Speed, value, _settingsService.Data.Common.Processing, (u, n) => u.Speed = n);
        }

        public bool NotificationsOnSuccessful
        {
            get => _settingsService.Data.Common.Notifications.OnSuccessful;
            set => SetProperty(_settingsService.Data.Common.Notifications.OnSuccessful, value, _settingsService.Data.Common.Notifications, (u, n) => u.OnSuccessful = n);
        }

        public bool NotificationsOnUnsuccessful
        {
            get => _settingsService.Data.Common.Notifications.OnUnsuccessful;
            set => SetProperty(_settingsService.Data.Common.Notifications.OnUnsuccessful, value, _settingsService.Data.Common.Notifications, (u, n) => u.OnUnsuccessful = n);
        }

        public string TempDirectory
        {
            get => _settingsService.Data.Common.Temp.Directory;
            set => SetProperty(_settingsService.Data.Common.Temp.Directory, value, _settingsService.Data.Common.Temp, (u, n) => u.Directory = n);
        }

        public bool TempDeleteOnFail
        {
            get => _settingsService.Data.Common.Temp.DeleteOnError;
            set => SetProperty(_settingsService.Data.Common.Temp.DeleteOnError, value, _settingsService.Data.Common.Temp, (u, n) => u.DeleteOnError = n);
        }

        public bool TwitchVodPassiveTrimming
        {
            get => _settingsService.Data.Twitch.Vod.PassiveTrimming;
            set => SetProperty(_settingsService.Data.Twitch.Vod.PassiveTrimming, value, _settingsService.Data.Twitch.Vod, (u, n) => u.PassiveTrimming = n);
        }

        public int TwitchVodParallelDownloads
        {
            get => _settingsService.Data.Twitch.Vod.MaxNumberOfParallelDownloads;
            set => SetProperty(_settingsService.Data.Twitch.Vod.MaxNumberOfParallelDownloads, value, _settingsService.Data.Twitch.Vod, (u, n) => u.MaxNumberOfParallelDownloads = n);
        }

        public bool TwitchVodChunkDownloadingErrorRetry
        {
            get => _settingsService.Data.Twitch.Vod.ChunkDownloadingError.Retry;
            set => SetProperty(_settingsService.Data.Twitch.Vod.ChunkDownloadingError.Retry, value, _settingsService.Data.Twitch.Vod.ChunkDownloadingError, (u, n) => u.Retry = n);
        }

        public int TwitchVodChunkDownloadingErrorRetryCount
        {
            get => _settingsService.Data.Twitch.Vod.ChunkDownloadingError.RetriesCount;
            set => SetProperty(_settingsService.Data.Twitch.Vod.ChunkDownloadingError.RetriesCount, value, _settingsService.Data.Twitch.Vod.ChunkDownloadingError, (u, n) => u.RetriesCount = n);
        }

        public int TwitchVodChunkDownloadingErrorRetryDelay
        {
            get => _settingsService.Data.Twitch.Vod.ChunkDownloadingError.RetryDelay;
            set => SetProperty(_settingsService.Data.Twitch.Vod.ChunkDownloadingError.RetryDelay, value, _settingsService.Data.Twitch.Vod.ChunkDownloadingError, (u, n) => u.RetryDelay = n);
        }

        [ObservableProperty]
        protected string _tasksFilenameTemplateTooltip;

        #endregion



        #region CONSTRUCTORS

        public SettingsViewModel(ISettingsService settingsService, IConfigurationService configurationService, IStoragePickerService storagePickerService)
        {
            _settingsService = settingsService;
            _configurationService = configurationService;
            _storagePickerService = storagePickerService;

            base.PropertyChanged += PropertyChangedEventHandler;

            _tasksFilenameTemplateTooltip = string.Join('\n', _configurationService.Common.FilenameTemplates.Select(x => StringResourcesManager.FilenameTemplate.Get(x.Name)));
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public async Task BrowseTasksDefaultOutputDirectory()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                this.TasksDefaultOutputDirectory = newDirectory;
            }
        }

        [RelayCommand]
        public async Task BrowseTempDirectory()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                this.TempDirectory = newDirectory;
            }
        }

        [RelayCommand]
        public async Task BrowseProcessingFFmpegLocation()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                this.ProcessingFFmpegLocation = newDirectory;
            }
        }

        [RelayCommand]
        public async Task RestoreToDefault()
        {
            await _settingsService.Restore();
            foreach (PropertyInfo property in this.GetType().GetProperties()) 
            {
                base.OnPropertyChanged(property.Name);
            }
        }

        #endregion



        #region PRIVATE METHODS

        private async void PropertyChangedEventHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            await _settingsService.Save();
        }

        #endregion
    }
}
