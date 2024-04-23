using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Settings;

namespace VDownload.Core.Tasks
{
    public interface IDownloadTaskManager
    {
        #region PROPERTIES

        ReadOnlyObservableCollection<DownloadTask> Tasks { get; }

        #endregion



        #region EVENTS

        event NotifyCollectionChangedEventHandler TaskCollectionChanged;

        #endregion



        #region METHODS

        DownloadTask AddTask(Video video, VideoDownloadOptions downloadOptions);
        void RemoveTask(DownloadTask task);

        #endregion
    }



    public class DownloadTaskManager : IDownloadTaskManager
    {
        #region SERVICES

        protected readonly IDownloadTaskFactoryService _downloadTaskFactoryService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region FIELDS

        private readonly Thread _taskMonitorThread;

        private readonly ObservableCollection<DownloadTask> _tasksMain;

        #endregion



        #region PROPERTIES

        public ReadOnlyObservableCollection<DownloadTask> Tasks { get; protected set; }

        #endregion



        #region EVENTS

        public event NotifyCollectionChangedEventHandler TaskCollectionChanged;

        #endregion



        #region CONSTRUCTORS

        public DownloadTaskManager(IDownloadTaskFactoryService downloadTaskFactoryService, ISettingsService settingsService)
        {
            _downloadTaskFactoryService = downloadTaskFactoryService;
            _settingsService = settingsService;

            _tasksMain = new ObservableCollection<DownloadTask>();
            _tasksMain.CollectionChanged += Tasks_CollectionChanged;
            Tasks = new ReadOnlyObservableCollection<DownloadTask>(_tasksMain);

            _taskMonitorThread = new Thread(TaskMonitor)
            {
                IsBackground = true
            };
            _taskMonitorThread.Start();
        }

        #endregion



        #region PUBLIC METHODS

        public DownloadTask AddTask(Video video, VideoDownloadOptions downloadOptions)
        {
            DownloadTask task = _downloadTaskFactoryService.Create(video, downloadOptions);
            _tasksMain.Add(task);
            return task;
        }

        public void RemoveTask(DownloadTask task)
        {
            _tasksMain.Remove(task);
        }

        #endregion



        #region PRIVATE METHODS

        private async void TaskMonitor()
        {
            await _settingsService.Load();

            DownloadTaskStatus[] pendingStatuses =
            [
                DownloadTaskStatus.Initializing,
                DownloadTaskStatus.Downloading,
                DownloadTaskStatus.Processing,
                DownloadTaskStatus.Finalizing
            ];
            while (true)
            {
                try
                {
                    IEnumerable<DownloadTask> pendingTasks = Tasks.Where(x => pendingStatuses.Contains(x.Status));
                    int freeSlots = _settingsService.Data.Common.Tasks.MaxNumberOfRunningTasks - pendingTasks.Count();
                    if (freeSlots > 0)
                    {
                        IEnumerable<DownloadTask> queuedTasks = Tasks.Where(x => x.Status == DownloadTaskStatus.Queued).OrderBy(x => x.CreateDate).Take(freeSlots);
                        foreach (DownloadTask queuedTask in queuedTasks)
                        {
                            queuedTask.Start();
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("TaskMonitor: Collection locked - skipping");
                }
            }
        }

        #endregion



        #region EVENT HANDLERS

        private void Tasks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TaskCollectionChanged?.Invoke(this, e);
        }

        #endregion
    }
}
