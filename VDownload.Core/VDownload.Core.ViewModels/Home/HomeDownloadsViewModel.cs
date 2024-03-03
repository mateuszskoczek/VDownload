using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Tasks;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.StringResources;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomeDownloadsViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IDownloadTaskManager _tasksManager;

        protected readonly IDialogsService _dialogsService;
        protected readonly IStringResourcesService _stringResourcesService;

        #endregion



        #region PROPERTIES

        public ReadOnlyObservableCollection<DownloadTask> Tasks => _tasksManager.Tasks;

        [ObservableProperty]
        private bool _taskListIsEmpty;

        #endregion



        #region CONSTRUCTORS

        public HomeDownloadsViewModel(IDownloadTaskManager tasksManager, IDialogsService dialogsService, IStringResourcesService stringResourcesService)
        {
            _tasksManager = tasksManager;
            _tasksManager.TaskCollectionChanged += Tasks_CollectionChanged;

            _dialogsService = dialogsService;
            _stringResourcesService = stringResourcesService;

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
                bool continueEnqueue = true;
                if (NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)
                {
                    string title = _stringResourcesService.CommonResources.Get("StartAtMeteredConnectionDialogTitle");
                    string message = _stringResourcesService.CommonResources.Get("StartAtMeteredConnectionDialogMessage");
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
