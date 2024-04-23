using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Strings;
using VDownload.Core.Tasks;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.Dialogs;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomeDownloadsViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IDownloadTaskManager _tasksManager;

        protected readonly IDialogsService _dialogsService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region PROPERTIES

        public ReadOnlyObservableCollection<DownloadTask> Tasks => _tasksManager.Tasks;

        [ObservableProperty]
        private bool _taskListIsEmpty;

        #endregion



        #region CONSTRUCTORS

        public HomeDownloadsViewModel(IDownloadTaskManager tasksManager, IDialogsService dialogsService, ISettingsService settingsService)
        {
            _tasksManager = tasksManager;
            _tasksManager.TaskCollectionChanged += Tasks_CollectionChanged;

            _dialogsService = dialogsService;
            _settingsService = settingsService;

            _taskListIsEmpty = _tasksManager.Tasks.Count == 0;
        }

        #endregion



        #region PUBLIC METHODS

        [RelayCommand]
        public async Task StartCancelTask(DownloadTask task)
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
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    string title = StringResourcesManager.HomeDownloadsView.Get("DialogErrorTitle");
                    string message = StringResourcesManager.HomeDownloadsView.Get("DialogErrorMessageNoInternetConnection");
                    await _dialogsService.ShowOk(title, message);
                    return;
                }

                bool continueEnqueue = true;
                if 
                (
                    _settingsService.Data.Common.Tasks.ShowMeteredConnectionWarnings
                    &&
                    NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection
                )
                {
                    string title = StringResourcesManager.Common.Get("StartAtMeteredConnectionDialogTitle");
                    string message = StringResourcesManager.Common.Get("StartAtMeteredConnectionDialogMessage");
                    DialogResultYesNo result = await _dialogsService.ShowYesNo(title, message);
                    continueEnqueue = result == DialogResultYesNo.Yes;
                }

                if (continueEnqueue)
                {
                    task.Enqueue();
                }
            }
            else
            {
                await task.Cancel();
            }
        }

        [RelayCommand]
        public async Task RemoveTask(DownloadTask task)
        {
            await task.Cancel();
            _tasksManager.RemoveTask(task);
        }

        #endregion



        #region PRIVATE METHODS

        private void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            TaskListIsEmpty = Tasks.Count == 0;
        }

        #endregion
    }
}
