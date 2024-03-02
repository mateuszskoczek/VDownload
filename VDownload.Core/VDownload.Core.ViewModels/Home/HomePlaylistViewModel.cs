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
using SimpleToolkit.MVVM;
using System.Text.RegularExpressions;

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

        protected List<VideoViewModel> _removedVideos;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected string _name;

        [ObservableProperty]
        protected ObservableDictionary<VideoViewModel, bool> _videos;

        [ObservableProperty]
        protected int _removedCount;

        [ObservableProperty]
        protected int _hiddenCount;

        [ObservableProperty]
        protected bool _isSomethingHidden;

        public string TitleFilter
        {
            get => _titleFilter;
            set
            {
                SetProperty(ref _titleFilter, value, nameof(TitleFilter));
                UpdateFilter();
            }
        }
        protected string _titleFilter;

        public string AuthorFilter
        {
            get => _authorFilter;
            set
            {
                SetProperty(ref _authorFilter, value, nameof(AuthorFilter));
                UpdateFilter();
            }
        }
        protected string _authorFilter;

        #endregion



        #region CONSTRUCTORS

        public HomePlaylistViewModel(IDownloadTaskManager tasksManager, ISettingsService settingsService, IStoragePickerService storagePickerService)
        {
            _tasksManager = tasksManager;
            _settingsService = settingsService;
            _storagePickerService = storagePickerService;

            _removedVideos = new List<VideoViewModel>();

            _videos = new ObservableDictionary<VideoViewModel, bool>();
        }

        #endregion



        #region PUBLIC METHODS

        public void LoadPlaylist(Playlist playlist)
        {
            _playlist = playlist;

            _removedVideos.Clear();

            _titleFilter = string.Empty;
            _authorFilter = string.Empty;
            Name = _playlist.Name;
            Videos.Clear();
            foreach (Video video in playlist)
            {
                Videos.Add(new VideoViewModel(video, _settingsService, _storagePickerService), true);
            }
            UpdateFilter();
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public async Task SelectDirectory()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                foreach (VideoViewModel video in Videos.Keys)
                {
                    video.DirectoryPath = newDirectory;
                }
            }
        }

        [RelayCommand]
        public void RemoveVideo(VideoViewModel video)
        {
            Videos[video] = false;

            _removedVideos.Add(video);

            UpdateFilter();
        }

        [RelayCommand]
        public void RestoreRemovedVideos()
        {
            foreach(VideoViewModel video in _removedVideos)
            {
                Videos[video] = true;
            }
            _removedVideos.Clear();

            UpdateFilter();
        }

        [RelayCommand]
        public void CreateTasksAndDownload() => CreateTasks(true);

        [RelayCommand]
        public void CreateTasks() => CreateTasks(false);

        #endregion



        #region PRIVATE METHODS

        protected void CreateTasks(bool download)
        {
            foreach (VideoViewModel video in Videos.Keys)
            {
                DownloadTask task = _tasksManager.AddTask(video.Video, video.BuildDownloadOptions());
                if (download)
                {
                    task.Enqueue();
                }
            }
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        protected void UpdateFilter()
        {
            Regex titleRegex = new Regex(TitleFilter);
            Regex authorRegex = new Regex(AuthorFilter);

            foreach (ObservableKeyValuePair<VideoViewModel, bool> item in Videos)
            {
                if (!titleRegex.IsMatch(item.Key.Title))
                {
                    item.Value = false;
                    continue;
                }

                if (!authorRegex.IsMatch(item.Key.Author))
                {
                    item.Value = false;
                    continue;
                }

                if (_removedVideos.Contains(item.Key))
                {
                    item.Value = false;
                    continue;
                }

                item.Value = true;
            }

            RemovedCount = _removedVideos.Count;
            HiddenCount = Videos.Values.Where(x => !x).Count();
            IsSomethingHidden = HiddenCount > 0;
        }

        #endregion



        #region EVENTS

        public event EventHandler CloseRequested;

        #endregion
    }
}
