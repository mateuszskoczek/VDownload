﻿using ABI.System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.ViewModels.Subscriptions.Helpers;
using VDownload.Models;
using VDownload.Services.Data.Subscriptions;
using VDownload.Services.UI.StringResources;
using VDownload.Sources;
using VDownload.Sources.Common;

namespace VDownload.Core.ViewModels.Subscriptions
{
    public partial class SubscriptionsViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly ISearchService _searchService;
        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly ISubscriptionsDataService _subscriptionsDataService;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected ObservableCollection<PlaylistViewModel> _playlists;

        [ObservableProperty]
        protected string _url;

        [ObservableProperty]
        protected bool _loading;

        [ObservableProperty]
        protected string _error;

        [ObservableProperty]
        protected bool _isErrorOpened;

        #endregion



        #region CONSTRUCTORS

        public SubscriptionsViewModel(ISearchService searchService, IStringResourcesService stringResourcesService, ISubscriptionsDataService subscriptionsDataService)
        {
            _searchService = searchService;
            _stringResourcesService = stringResourcesService;
            _subscriptionsDataService = subscriptionsDataService;

            _playlists = new ObservableCollection<PlaylistViewModel>();
            _loading = false;
            _isErrorOpened = true;
            _error = null;
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public void Navigation()
        {
            Playlists.Clear();
            foreach (Subscription sub in _subscriptionsDataService.Data)
            {
                Playlists.Add(new PlaylistViewModel(sub.Name, sub.Source, sub.Guid));
            }
        }

        [RelayCommand]
        public async Task RemovePlaylist(PlaylistViewModel playlist)
        {
            Playlists.Remove(playlist);

            Subscription sub = _subscriptionsDataService.Data.FirstOrDefault(x => x.Guid == playlist.Guid);
            _subscriptionsDataService.Data.Remove(sub);
            await _subscriptionsDataService.Save();
        }

        [RelayCommand]
        public async Task Add()
        {
            Loading = true;

            Playlist playlist;
            try
            {
                playlist = await _searchService.SearchPlaylist(Url, 0);
            }
            catch (MediaSearchException ex)
            {
                Error = _stringResourcesService.SearchResources.Get(ex.StringCode);
                Loading = false;
                return;
            }


            if (_subscriptionsDataService.Data.Any(x => x.Source == playlist.Source && x.Name == playlist.Name))
            {
                Error = _stringResourcesService.SubscriptionsViewResources.Get("DuplicateError");
                Loading = false;
                return;
            }

            Subscription subscription = new Subscription
            {
                Name = playlist.Name,
                Source = playlist.Source,
                Url = playlist.Url,
            };
            playlist.ForEach(x => subscription.VideoIds.Add(x.Id));

            _subscriptionsDataService.Data.Add(subscription);
            await _subscriptionsDataService.Save();
            Playlists.Add(new PlaylistViewModel(subscription.Name, subscription.Source, subscription.Guid));

            Loading = false;
        }

        [RelayCommand]
        public void CloseError()
        {
            Error = null;
            IsErrorOpened = true;
        }

        #endregion 
    }
}
