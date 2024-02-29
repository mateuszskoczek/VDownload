using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Tasks;
using VDownload.Core.ViewModels.Home.Helpers;
using VDownload.Models;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.StoragePicker;
using VDownload.Sources.Twitch.Configuration.Models;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomePlaylistViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IDownloadTaskManager _tasksManager;

        protected readonly ISettingsService _settingsService;
        protected readonly IStoragePickerService _storagePickerService;

        #endregion



        #region FIELDS

        protected Playlist _playlist;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected string _name;

        [ObservableProperty]
        protected ObservableCollection<VideoViewModel> _videos;

        #endregion



        #region CONSTRUCTORS

        public HomePlaylistViewModel(IDownloadTaskManager tasksManager, ISettingsService settingsService, IStoragePickerService storagePickerService)
        {
            _tasksManager = tasksManager;
            _settingsService = settingsService;
            _storagePickerService = storagePickerService;

            _videos = new ObservableCollection<VideoViewModel>();
        }

        #endregion



        #region PUBLIC METHODS

        public async Task LoadPlaylist(Playlist playlist)
        {
            _playlist = playlist;

            Videos.Clear();

            Name = _playlist.Name;
            foreach (Video video in playlist)
            {
                Videos.Add(new VideoViewModel(video, _settingsService, _storagePickerService));
            }
        }

        #endregion



        #region COMMANDS



        #endregion



        #region EVENTS

        public event EventHandler CloseRequested;

        #endregion
    }
}
