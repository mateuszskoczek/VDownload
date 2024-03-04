using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Data.Settings;

namespace VDownload.Core.ViewModels.Settings
{
    public class SettingsViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly ISettingsService _settingsService;

        #endregion



        #region PROPERTIES

        public int SearchingPlaylistCount
        {
            get => _settingsService.Data.Common.Searching.MaxNumberOfVideosToGetFromPlaylist;
            set => SetProperty(_settingsService.Data.Common.Searching.MaxNumberOfVideosToGetFromPlaylist, value, _settingsService.Data.Common.Searching, (u, n) => u.MaxNumberOfVideosToGetFromPlaylist = n);
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

        #endregion



        #region CONSTRUCTORS

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            base.PropertyChanged += PropertyChangedEventHandler;
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
