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
using VDownload.Services.Utility.Filename;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.StringResources;
using CommunityToolkit.WinUI.Helpers;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomePlaylistViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IDownloadTaskManager _tasksManager;

        protected readonly ISettingsService _settingsService;
        protected readonly IStoragePickerService _storagePickerService;
        protected readonly IFilenameService _filenameService;
        protected readonly IDialogsService _dialogsService;
        protected readonly IStringResourcesService _stringResourcesService;

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

        public long MinViewsFilter
        {
            get => _minViewsFilter;
            set
            {
                SetProperty(ref _minViewsFilter, value, nameof(MinViewsFilter));
                UpdateFilter();
            }
        }
        protected long _minViewsFilter;

        public long MaxViewsFilter
        {
            get => _maxViewsFilter;
            set
            {
                SetProperty(ref _maxViewsFilter, value, nameof(MaxViewsFilter));
                UpdateFilter();
            }
        }
        protected long _maxViewsFilter;

        [ObservableProperty]
        protected long _minViews;

        [ObservableProperty]
        protected long _maxViews;

        public DateTimeOffset MinDateFilter
        {
            get => _minDateFilter;
            set
            {
                SetProperty(ref _minDateFilter, value, nameof(MinDateFilter));
                UpdateFilter();
            }
        }
        protected DateTimeOffset _minDateFilter;

        public DateTimeOffset MaxDateFilter
        {
            get => _maxDateFilter;
            set
            {
                SetProperty(ref _maxDateFilter, value, nameof(MaxDateFilter));
                UpdateFilter();
            }
        }
        protected DateTimeOffset _maxDateFilter;

        [ObservableProperty]
        protected DateTimeOffset _minDate;

        [ObservableProperty]
        protected DateTimeOffset _maxDate;


        public TimeSpan MinDurationFilter
        {
            get => _minDurationFilter;
            set
            {
                SetProperty(ref _minDurationFilter, value, nameof(MinDurationFilter));
                UpdateFilter();
            }
        }
        protected TimeSpan _minDurationFilter;

        public TimeSpan MaxDurationFilter
        {
            get => _maxDurationFilter;
            set
            {
                SetProperty(ref _maxDurationFilter, value, nameof(MaxDurationFilter));
                UpdateFilter();
            }
        }
        protected TimeSpan _maxDurationFilter;

        [ObservableProperty]
        protected TimeSpan _minDuration;

        [ObservableProperty]
        protected TimeSpan _maxDuration;

        #endregion



        #region CONSTRUCTORS

        public HomePlaylistViewModel(IDownloadTaskManager tasksManager, ISettingsService settingsService, IStoragePickerService storagePickerService, IFilenameService filenameService, IDialogsService dialogsService, IStringResourcesService stringResourcesService)
        {
            _tasksManager = tasksManager;
            _settingsService = settingsService;
            _storagePickerService = storagePickerService;
            _filenameService = filenameService;
            _dialogsService = dialogsService;
            _stringResourcesService = stringResourcesService;

            _removedVideos = new List<VideoViewModel>();

            _videos = new ObservableDictionary<VideoViewModel, bool>();
        }

        #endregion



        #region PUBLIC METHODS

        public void LoadPlaylist(Playlist playlist)
        {
            _playlist = playlist;
            ParallelQuery<Video> playlistQuery = playlist.AsParallel();

            _removedVideos.Clear();

            _titleFilter = string.Empty;
            _authorFilter = string.Empty;

            IEnumerable<long> views = playlistQuery.Select(x => x.Views);
            MinViews = views.Min();
            MaxViews = views.Max();
            _minViewsFilter = MinViews;
            _maxViewsFilter = MaxViews;

            IEnumerable<DateTimeOffset> date = playlist.Select(x => new DateTimeOffset(x.PublishDate));
            MinDate = date.Min();
            MaxDate = date.Max();
            _minDateFilter = MinDate;
            _maxDateFilter = MaxDate;

            IEnumerable<TimeSpan> duration = playlistQuery.Select(x => x.Duration);
            MinDuration = duration.Min();
            MaxDuration = duration.Max();
            _minDurationFilter = MinDuration;
            _maxDurationFilter = MaxDuration;

            Name = _playlist.Name;
            Videos.Clear();
            foreach (Video video in playlist)
            {
                Videos.Add(new VideoViewModel(video, _settingsService, _storagePickerService, _filenameService), true);
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
        public async Task CreateTasksAndDownload() => await CreateTasks(true);

        [RelayCommand]
        public async Task CreateTasks() => await CreateTasks(false);

        #endregion



        #region PRIVATE METHODS

        protected async Task CreateTasks(bool download)
        {
            if (download && NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
            {
                string title = _stringResourcesService.CommonResources.Get("StartAtMeteredConnectionDialogTitle");
                string message = _stringResourcesService.CommonResources.Get("StartAtMeteredConnectionDialogMessage");
                DialogResultYesNo result = await _dialogsService.ShowYesNo(title, message);
                download = result == DialogResultYesNo.Yes;
            }

            IEnumerable<VideoViewModel> videos = Videos.Cast<ObservableKeyValuePair<VideoViewModel, bool>>()
                                                       .Where(x => x.Value)
                                                       .Select(x => x.Key);

            foreach (VideoViewModel video in videos)
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
                VideoViewModel video = item.Key;
                bool hide = 
                (
                    _removedVideos.Contains(video)
                    ||
                    !titleRegex.IsMatch(video.Title)
                    ||
                    !authorRegex.IsMatch(video.Author)
                    ||
                    MinViewsFilter > video.Views
                    ||
                    MaxViewsFilter < video.Views
                    ||
                    MinDateFilter.Date > video.PublishDate.Date
                    ||
                    MaxDateFilter.Date < video.PublishDate.Date
                    ||
                    MinDurationFilter > video.Duration
                    ||
                    MaxDurationFilter < video.Duration
                );
                item.Value = !hide;
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
