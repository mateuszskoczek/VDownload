using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Services.Data.Subscriptions;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.StringResources;
using VDownload.Sources;
using VDownload.Sources.Common;
using VDownload.Sources.Twitch.Configuration.Models;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomeViewModel : ObservableObject
    {
        #region ENUMS

        public enum OptionBarMessageIconType
        {
            None,
            ProgressRing,
            Error
        }

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
        protected readonly ISubscriptionsDataService _subscriptionsDataService;
        protected readonly IDialogsService _dialogsService;

        protected readonly IDownloadTaskManager _downloadTaskManager;

        protected readonly HomeVideoViewModel _videoViewModel;
        protected readonly HomeVideoCollectionViewModel _videoCollectionViewModel;

        #endregion



        #region FIELDS

        protected readonly Type _downloadsView = typeof(HomeDownloadsViewModel);
        protected readonly Type _videoView = typeof(HomeVideoViewModel);
        protected readonly Type _videoCollectionView = typeof(HomeVideoCollectionViewModel);

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private Type _mainContent;


        [ObservableProperty]
        private OptionBarContentType _optionBarContent;

        [ObservableProperty]
        private string _optionBarMessage;

        [ObservableProperty]
        private OptionBarMessageIconType _optionBarMessageIcon;

        [ObservableProperty]
        private bool _optionBarLoadSubscriptionButtonChecked;

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

        public HomeViewModel(IConfigurationService configurationService, ISettingsService settingsService, IStringResourcesService stringResourcesService, ISearchService searchService, ISubscriptionsDataService subscriptionsDataService, IDialogsService dialogsService, IDownloadTaskManager downloadTaskManager, HomeVideoViewModel videoViewModel, HomeVideoCollectionViewModel videoCollectionViewModel)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;
            _stringResourcesService = stringResourcesService;
            _searchService = searchService;
            _subscriptionsDataService = subscriptionsDataService;
            _dialogsService = dialogsService;

            _downloadTaskManager = downloadTaskManager;

            _videoViewModel = videoViewModel;
            _videoViewModel.CloseRequested += BackToDownload_EventHandler;

            _videoCollectionViewModel = videoCollectionViewModel;
            _videoCollectionViewModel.CloseRequested += BackToDownload_EventHandler;
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public async Task Navigation()
        {
            await _settingsService.Load();

            MainContent = _downloadsView;

            OptionBarContent = OptionBarContentType.None;
            OptionBarMessageIcon = OptionBarMessageIconType.None;
            OptionBarMessage = null;
            OptionBarLoadSubscriptionButtonChecked = false;
            OptionBarVideoSearchButtonChecked = false;
            OptionBarPlaylistSearchButtonChecked = false;
            OptionBarSearchNotPending = true;
            OptionBarVideoSearchTBValue = string.Empty;
            OptionBarPlaylistSearchNBValue = _settingsService.Data.Common.Searching.MaxNumberOfVideosToGetFromPlaylist;
            OptionBarPlaylistSearchTBValue = string.Empty;
        }

        [RelayCommand]
        public async Task LoadFromSubscription()
        {
            MainContent = _downloadsView;
            OptionBarMessageIcon = OptionBarMessageIconType.None;
            OptionBarMessage = null;

            if (OptionBarLoadSubscriptionButtonChecked)
            {
                OptionBarContent = OptionBarContentType.None;
                OptionBarVideoSearchButtonChecked = false;
                OptionBarPlaylistSearchButtonChecked = false;

                OptionBarSearchNotPending = false;
                OptionBarMessageIcon = OptionBarMessageIconType.ProgressRing;
                OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageLoading");

                SubscriptionsVideoList subList = new SubscriptionsVideoList { Name = _stringResourcesService.CommonResources.Get("SubscriptionVideoListName") };
                List<Task> tasks = new List<Task>();
                foreach (Subscription sub in _subscriptionsDataService.Data.ToArray())
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        Playlist playlist;
                        try
                        {
                            playlist = await _searchService.SearchPlaylist(sub.Url.OriginalString, 0);
                        }
                        catch (MediaSearchException)
                        {
                            return;
                        }

                        IEnumerable<Video> newIds = playlist.Where(x => !sub.VideoIds.Contains(x.Id));
                        subList.AddRange(newIds);
                        foreach (Video video in newIds)
                        {
                            sub.VideoIds.Add(video.Id);
                        }
                    }));
                }
                await Task.WhenAll(tasks);
                await _subscriptionsDataService.Save();

                if (subList.Count > 0)
                {
                    OptionBarMessage = $"{_stringResourcesService.HomeViewResources.Get("OptionBarMessageVideosFound")} {subList.Count}";

                    _videoCollectionViewModel.LoadCollection(subList);
                    MainContent = _videoCollectionView;
                }
                else
                {
                    OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageVideosNotFound");
                }

                OptionBarSearchNotPending = true;
                OptionBarMessageIcon = OptionBarMessageIconType.None;
            }
        }

        [RelayCommand]
        public void VideoSearchShow()
        {
            OptionBarSearchNotPending = true;
            OptionBarMessageIcon = OptionBarMessageIconType.None;
            OptionBarMessage = null;
            MainContent = _downloadsView;

            if (OptionBarContent != OptionBarContentType.VideoSearch)
            {
                OptionBarContent = OptionBarContentType.VideoSearch;
                OptionBarPlaylistSearchButtonChecked = false;
                OptionBarLoadSubscriptionButtonChecked = false;
            }
            else
            {
                OptionBarContent = OptionBarContentType.None;
            }
        }

        [RelayCommand]
        public void PlaylistSearchShow()
        {
            OptionBarSearchNotPending = true;
            OptionBarMessageIcon = OptionBarMessageIconType.None;
            OptionBarMessage = null;
            MainContent = _downloadsView;

            if (OptionBarContent != OptionBarContentType.PlaylistSearch)
            {
                OptionBarContent = OptionBarContentType.PlaylistSearch;
                OptionBarVideoSearchButtonChecked = false;
                OptionBarLoadSubscriptionButtonChecked = false;
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
            OptionBarMessageIcon = OptionBarMessageIconType.ProgressRing;
            OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageLoading");

            Video video;
            try
            {
                video = await _searchService.SearchVideo(OptionBarVideoSearchTBValue);
            }
            catch (MediaSearchException ex)
            {
                OptionBarMessageIcon = OptionBarMessageIconType.Error;
                OptionBarMessage = _stringResourcesService.SearchResources.Get(ex.StringCode);
                OptionBarSearchNotPending = true;
                return;
            }

            await _videoViewModel.LoadVideo(video);

            MainContent = _videoView;

            OptionBarSearchNotPending = true;
            OptionBarMessageIcon = OptionBarMessageIconType.None;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public async Task PlaylistSearchStart()
        {
            OptionBarSearchNotPending = false;
            OptionBarMessageIcon = OptionBarMessageIconType.ProgressRing;
            OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageLoading");

            Playlist playlist;
            try
            {
                playlist = await _searchService.SearchPlaylist(OptionBarPlaylistSearchTBValue, OptionBarPlaylistSearchNBValue);
            }
            catch (MediaSearchException ex)
            {
                OptionBarMessageIcon = OptionBarMessageIconType.Error;
                OptionBarMessage = _stringResourcesService.SearchResources.Get(ex.StringCode);
                OptionBarSearchNotPending = true;
                return;
            }

            _videoCollectionViewModel.LoadCollection(playlist);

            MainContent = _videoCollectionView;

            OptionBarSearchNotPending = true;
            OptionBarMessageIcon = OptionBarMessageIconType.None;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public async Task Download()
        {
            if (_downloadTaskManager.Tasks.Count > 0 && NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                string title = _stringResourcesService.CommonResources.Get("StartAtMeteredConnectionDialogTitle");
                string message = _stringResourcesService.CommonResources.Get("StartAtMeteredConnectionDialogMessage");
                DialogResultYesNo result = await _dialogsService.ShowYesNo(title, message);
                if (result == DialogResultYesNo.No)
                {
                    return;
                }
            }

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
