using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private string _optionBarError;

        [ObservableProperty]
        private bool _optionBarIsErrorOpened;

        [ObservableProperty]
        private OptionBarContentType _optionBarContent;

        [ObservableProperty]
        private string _optionBarMessage;

        [ObservableProperty]
        private bool _optionBarLoading;

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
            OptionBarLoading = false;
            OptionBarMessage = null;
            OptionBarError = null;
            OptionBarIsErrorOpened = true;
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
            SearchButtonClicked();

            if (OptionBarLoadSubscriptionButtonChecked)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    ShowError(ErrorNoInternetConnection());
                    OptionBarLoadSubscriptionButtonChecked = false;
                    return;
                }

                OptionBarContent = OptionBarContentType.None;
                OptionBarVideoSearchButtonChecked = false;
                OptionBarPlaylistSearchButtonChecked = false;

                StartSearch();

                SubscriptionsVideoList subList = new SubscriptionsVideoList { Name = _stringResourcesService.CommonResources.Get("SubscriptionVideoListName") };
                List<Task> tasks = new List<Task>();
                try
                {
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
                }
                catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
                {
                    ShowError(ErrorSearchTimeout());
                    return;
                }

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
                OptionBarLoading = false;
            }
        }

        [RelayCommand]
        public void VideoSearchShow()
        {
            SearchButtonClicked();

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
            SearchButtonClicked();

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
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                ShowError(ErrorNoInternetConnection());
                return;
            }

            StartSearch();

            Video video;
            try
            {
                video = await _searchService.SearchVideo(OptionBarVideoSearchTBValue);
            }
            catch (MediaSearchException ex)
            {
                ShowError(_stringResourcesService.SearchResources.Get(ex.StringCode));
                return;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
            {
                ShowError(ErrorSearchTimeout());
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
            if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
            {
                ShowError(ErrorNoInternetConnection());
                return;
            }

            StartSearch();

            Playlist playlist;
            try
            {
                playlist = await _searchService.SearchPlaylist(OptionBarPlaylistSearchTBValue, OptionBarPlaylistSearchNBValue);
            }
            catch (MediaSearchException ex)
            {
                ShowError(_stringResourcesService.SearchResources.Get(ex.StringCode));
                return;
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
            {
                ShowError(ErrorSearchTimeout());
                return;
            }

            _videoCollectionViewModel.LoadCollection(playlist);

            MainContent = _videoCollectionView;

            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public async Task Download()
        {
            if (_downloadTaskManager.Tasks.Count > 0)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    ShowError(ErrorNoInternetConnection());
                    return;
                }

                if 
                (
                    _settingsService.Data.Common.Tasks.ShowMeteredConnectionWarnings 
                    && 
                    NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection
                )
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
        }

        [RelayCommand]
        public async Task Cancel()
        {
            List<Task> tasks = new List<Task>();
            foreach (DownloadTask task in _downloadTaskManager.Tasks)
            {
                tasks.Add(task.Cancel());
            }
            await Task.WhenAll(tasks);
        }

        [RelayCommand]
        public void CloseError()
        {
            OptionBarError = null;
            OptionBarIsErrorOpened = true;
        }

        #endregion



        #region PRIVATE METHODS

        protected void ShowError(string message)
        {
            OptionBarError = message;
            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
        }

        protected void SearchButtonClicked()
        {
            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
            MainContent = _downloadsView;
        }

        protected void StartSearch()
        {
            OptionBarSearchNotPending = false;
            OptionBarLoading = true;
            OptionBarMessage = _stringResourcesService.HomeViewResources.Get("OptionBarMessageLoading");
        }

        protected async void BackToDownload_EventHandler(object sender, EventArgs e) => await Navigation();

        protected string ErrorNoInternetConnection() => _stringResourcesService.HomeViewResources.Get("ErrorInfoBarNoInternetConnection");

        protected string ErrorSearchTimeout() => _stringResourcesService.SearchResources.Get("SearchTimeout");

        #endregion
    }
}
