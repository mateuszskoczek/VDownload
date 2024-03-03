using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.StringResources;
using VDownload.Sources;
using VDownload.Sources.Common;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomeViewModel : ObservableObject
    {
        #region ENUMS

        public enum OptionBarContentType
        {
            None,
            VideoSearch,
            PlaylistSearch
        }

        public enum MainContentType
        {
            Downloads,
            Video
        }

        #endregion



        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;
        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly ISearchService _searchService;

        protected readonly IDownloadTaskManager _downloadTaskManager;

        protected readonly HomeVideoViewModel _videoViewModel;
        protected readonly HomePlaylistViewModel _playlistViewModel;

        #endregion



        #region FIELDS

        protected readonly Type _downloadsView = typeof(HomeDownloadsViewModel);
        protected readonly Type _videoView = typeof(HomeVideoViewModel);
        protected readonly Type _playlistView = typeof(HomePlaylistViewModel);

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private Type _mainContent;


        [ObservableProperty]
        private OptionBarContentType _optionBarContent;

        [ObservableProperty]
        private string _optionBarMessage;

        [ObservableProperty]
        private bool _optionBarLoading;

        [ObservableProperty]
        private bool _optionBarVideoSearchButtonChecked;

        [ObservableProperty]
        private bool _optionBarPlaylistSearchButtonChecked;

        [ObservableProperty]
        private bool _optionBarSearchNotPending;

        [ObservableProperty]
        private string _optionBarVideoSearchTBValue;

        [ObservableProperty]
        private string _optionBarPlaylistSearchTBValue;

        [ObservableProperty]
        private int _optionBarPlaylistSearchNBValue;

        #endregion



        #region CONSTRUCTORS

        public HomeViewModel(IConfigurationService configurationService, ISettingsService settingsService, IStringResourcesService stringResourcesService, ISearchService searchService, IDownloadTaskManager downloadTaskManager, HomeVideoViewModel videoViewModel, HomePlaylistViewModel playlistViewModel)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;
            _stringResourcesService = stringResourcesService;
            _searchService = searchService;

            _downloadTaskManager = downloadTaskManager;

            _videoViewModel = videoViewModel;
            _videoViewModel.CloseRequested += BackToDownload_EventHandler;

            _playlistViewModel = playlistViewModel;
            _playlistViewModel.CloseRequested += BackToDownload_EventHandler;
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public async Task Navigation()
        {
            await _settingsService.Load();

            MainContent = _downloadsView;

            OptionBarContent = OptionBarContentType.None;
            OptionBarMessage = null;
            OptionBarVideoSearchButtonChecked = false;
            OptionBarPlaylistSearchButtonChecked = false;
            OptionBarSearchNotPending = true;
            OptionBarVideoSearchTBValue = string.Empty;
            OptionBarPlaylistSearchNBValue = _settingsService.Data.Common.MaxNumberOfVideosToGetFromPlaylist;
            OptionBarPlaylistSearchTBValue = string.Empty;
        }

        [RelayCommand]
        public void LoadFromSubscription()
        {
            MainContent = _downloadsView;

            OptionBarContent = OptionBarContentType.None;
            OptionBarVideoSearchButtonChecked = false;
            OptionBarPlaylistSearchButtonChecked = false;
            OptionBarSearchNotPending = false;

            //TODO: Load videos
        }

        [RelayCommand]
        public void VideoSearchShow()
        {
            MainContent = _downloadsView;

            if (OptionBarContent != OptionBarContentType.VideoSearch)
            {
                OptionBarContent = OptionBarContentType.VideoSearch;
                OptionBarPlaylistSearchButtonChecked = false;
            }
            else
            {
                OptionBarContent = OptionBarContentType.None;
            }
        }

        [RelayCommand]
        public void PlaylistSearchShow()
        {
            MainContent = _downloadsView;

            if (OptionBarContent != OptionBarContentType.PlaylistSearch)
            {
                OptionBarContent = OptionBarContentType.PlaylistSearch;
                OptionBarVideoSearchButtonChecked = false;
            }
            else
            {
                OptionBarContent = OptionBarContentType.None;
            }
        }

        [RelayCommand]
        public async Task VideoSearchStart()
        {
            OptionBarSearchNotPending = false;
            OptionBarLoading = true;
            OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageLoading");

            Video video;
            try
            {
                video = await _searchService.SearchVideo(OptionBarVideoSearchTBValue);
            }
            catch (MediaSearchException ex)
            {
                OptionBarLoading = false;
                OptionBarMessage = _stringResourcesService.SearchResources.Get(ex.StringCode);
                OptionBarSearchNotPending = true;
                return;
            }

            await _videoViewModel.LoadVideo(video);

            MainContent = _videoView;

            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public async Task PlaylistSearchStart()
        {
            OptionBarSearchNotPending = false;
            OptionBarLoading = true;
            OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageLoading");

            Playlist playlist;
            try
            {
                playlist = await _searchService.SearchPlaylist(OptionBarPlaylistSearchTBValue, OptionBarPlaylistSearchNBValue);
            }
            catch (MediaSearchException ex)
            {
                OptionBarLoading = false;
                OptionBarMessage = _stringResourcesService.SearchResources.Get(ex.StringCode);
                OptionBarSearchNotPending = true;
                return;
            }

            _playlistViewModel.LoadPlaylist(playlist);

            MainContent = _playlistView;

            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public void Download()
        {
            foreach (DownloadTask task in _downloadTaskManager.Tasks)
            {
                task.Enqueue();
            }
        }

        #endregion



        #region PRIVATE METHODS

        private async void BackToDownload_EventHandler(object sender, EventArgs e) => await Navigation();

        #endregion
    }
}
