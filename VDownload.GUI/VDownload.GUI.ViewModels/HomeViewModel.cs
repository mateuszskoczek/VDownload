using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Common;
using VDownload.Common.Exceptions;
using VDownload.Common.Models;
using VDownload.GUI.Services.StoragePicker;
using VDownload.Services.Search;
using VDownload.Tasks;

namespace VDownload.GUI.ViewModels
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

        private IStoragePickerService _storagePickerService;
        private ISearchService _searchService;
        private IDownloadTasksManager _tasksService;

        #endregion



        #region PROPERTIES

        // MAIN

        [ObservableProperty]
        private MainContentType _mainContent;


        // DOWNLOADS
        public ObservableCollection<DownloadTask> Tasks => _tasksService.Tasks;

        [ObservableProperty]
        private bool _taskListIsEmpty;


        // VIDEO

        [ObservableProperty]
        private DownloadTask _task;


        // OPTION BAR

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

        public HomeViewModel(IStoragePickerService storagePickerService, ISearchService searchService, IDownloadTasksManager tasksService) 
        {
            _storagePickerService = storagePickerService;
            _searchService = searchService;
            _tasksService = tasksService;
            _tasksService.Tasks.CollectionChanged += Tasks_CollectionChanged;

            _taskListIsEmpty = _tasksService.Tasks.Count == 0;
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public void Navigation()
        {
            MainContent = MainContentType.Downloads;

            OptionBarContent = OptionBarContentType.None;
            OptionBarMessage = null;
            OptionBarVideoSearchButtonChecked = false;
            OptionBarPlaylistSearchButtonChecked = false;
            OptionBarSearchNotPending = true;
            OptionBarVideoSearchTBValue = string.Empty;
            OptionBarPlaylistSearchNBValue = 1; // TODO: load from settings
            OptionBarPlaylistSearchTBValue = string.Empty;
        }


        // DOWNLOADS

        [RelayCommand]
        public void StartCancelTask(DownloadTask task)
        {
            DownloadTaskStatus[] idleStatuses =
            [
                DownloadTaskStatus.Idle,
                DownloadTaskStatus.EndedUnsuccessfully,
                DownloadTaskStatus.EndedSuccessfully,
                DownloadTaskStatus.EndedCancelled
            ];
            if (idleStatuses.Contains(task.Status))
            {
                task.Enqueue();
            }
            else
            {
                task.Cancel();
            }
        }


        // VIDEO

        [RelayCommand]
        public async Task Browse()
        {
            string? newDirectory = await _storagePickerService.OpenDirectory();
            if (newDirectory is not null)
            {
                Task.DirectoryPath = newDirectory;
            }
        }

        [RelayCommand]
        public async Task CreateTask()
        {
            string extension = Task.MediaType switch
            {
                MediaType.OnlyAudio => Task.AudioExtension.ToString(),
                _ => Task.VideoExtension.ToString()
            };

            Task.Filename = string.Join("_", Task.Filename.Split(Path.GetInvalidFileNameChars()));
            string file = $"{Task.Filename}.{extension}";
            string path = Path.Combine(Task.DirectoryPath, file);

            await File.WriteAllBytesAsync(path, [0x00]);
            File.Delete(path);

            _tasksService.AddTask(Task);

            Navigation();
        }


        // OPTION BAR

        [RelayCommand]
        public void LoadFromSubscription()
        {
            MainContent = MainContentType.Downloads;

            OptionBarContent = OptionBarContentType.None;
            OptionBarVideoSearchButtonChecked = false;
            OptionBarPlaylistSearchButtonChecked = false;
            OptionBarSearchNotPending = false;

            //TODO: Load videos
        }

        [RelayCommand]
        public void VideoSearchShow()
        {
            MainContent = MainContentType.Downloads;

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
            MainContent = MainContentType.Downloads;

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
            OptionBarMessage = "Loading...";

            Video video;
            try
            {
                video = await _searchService.SearchVideo(OptionBarVideoSearchTBValue);
            }
            catch (MediaSearchException ex)
            {
                OptionBarLoading = false;
                OptionBarMessage = ex.Message;
                OptionBarSearchNotPending = true;
                return;
            }

            Task = new DownloadTask(video);

            MainContent = MainContentType.Video;

            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public async Task PlaylistSearchStart()
        {
            OptionBarSearchNotPending = false;
            OptionBarLoading = true;
            OptionBarMessage = "Loading...";

            Playlist playlist;
            try
            {
                playlist = await _searchService.SearchPlaylist(OptionBarPlaylistSearchTBValue, OptionBarPlaylistSearchNBValue);
            }
            catch (MediaSearchException ex)
            {
                OptionBarLoading = false;
                OptionBarMessage = ex.Message;
                OptionBarSearchNotPending = true;
                return;
            }

            OptionBarSearchNotPending = true;
            OptionBarLoading = false;
            OptionBarMessage = null;
        }

        [RelayCommand]
        public void Download()
        {

        }

        #endregion



        #region EVENT HANDLERS

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TaskListIsEmpty = Tasks.Count == 0;
        }

        #endregion
    }
}
