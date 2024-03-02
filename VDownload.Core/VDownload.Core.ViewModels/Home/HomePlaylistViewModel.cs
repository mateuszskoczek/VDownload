using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        protected Dictionary<VideoViewModel, bool> _allVideos;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected string _name;

        [ObservableProperty]
        protected ObservableCollection<VideoViewModel> _videos;

        [ObservableProperty]
        protected int _removedCount;

        #endregion



        #region CONSTRUCTORS

        public HomePlaylistViewModel(IDownloadTaskManager tasksManager, ISettingsService settingsService, IStoragePickerService storagePickerService)
        {
            _tasksManager = tasksManager;
            _settingsService = settingsService;
            _storagePickerService = storagePickerService;

            _allVideos = new Dictionary<VideoViewModel, bool>();
            _videos = new ObservableCollection<VideoViewModel>();
        }

        #endregion



        #region PUBLIC METHODS

        public void LoadPlaylist(Playlist playlist)
        {
            _playlist = playlist;

            _allVideos.Clear();
            foreach (Video video in playlist)
            {
                _allVideos.Add(new VideoViewModel(video, _settingsService, _storagePickerService), false);
            }

            Name = _playlist.Name;
            Videos = new ObservableCollection<VideoViewModel>(_allVideos.Keys);
            RemovedCount = 0;
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public async Task SelectDirectory()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                foreach (VideoViewModel video in _allVideos.Keys)
                {
                    video.DirectoryPath = newDirectory;
                }
            }
        }

        [RelayCommand]
        public void RemoveVideo(VideoViewModel video)
        {
            _allVideos[video] = true;

            Videos.Remove(video);

            RemovedCount = _allVideos.Where(x => x.Value).Count();
        }

        [RelayCommand]
        public void RestoreRemovedVideos()
        {
            foreach(VideoViewModel video in _allVideos.Where(x => x.Value == true).Select(x => x.Key))
            {
                _allVideos[video] = false;
                Videos.Add(video);
            }
            RemovedCount = _allVideos.Where(x => x.Value).Count();
        }

        [RelayCommand]
        public void CreateTasksAndDownload() => CreateTasks(true);

        [RelayCommand]
        public void CreateTasks() => CreateTasks(false);

        #endregion



        #region PRIVATE METHODS

        protected void CreateTasks(bool download)
        {
            foreach (VideoViewModel video in Videos)
            {
                DownloadTask task = _tasksManager.AddTask(video.Video, video.BuildDownloadOptions());
                if (download)
                {
                    task.Enqueue();
                }
            }
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion



        #region EVENTS

        public event EventHandler CloseRequested;

        #endregion
    }
}
