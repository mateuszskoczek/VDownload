using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Common;

namespace VDownload.Tasks
{ 
    public interface IDownloadTasksManager
    {
        #region PROPERTIES

        ObservableCollection<DownloadTask> Tasks { get; }

        #endregion



        #region METHODS

        void AddTask(DownloadTask task);

        #endregion
    }



    public class DownloadTasksManager : IDownloadTasksManager
    {
        #region FIELDS

        private readonly Task _taskMonitor;

        #endregion



        #region PROPERTIES

        public ObservableCollection<DownloadTask> Tasks { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public DownloadTasksManager()
        {
            Tasks = new ObservableCollection<DownloadTask>();

            _taskMonitor = Task.Run(TaskMonitor);
        }

        #endregion



        #region PUBLIC METHODS

        public void AddTask(DownloadTask task)
        {
            Tasks.Add(task);
        }

        #endregion



        #region PRIVATE METHODS

        private void TaskMonitor()
        {
            int maxTaskNumber = 5; //TODO: from settings
            DownloadTaskStatus[] pendingStatuses =
            [
                DownloadTaskStatus.Initializing,
                DownloadTaskStatus.Downloading,
                DownloadTaskStatus.Processing,
                DownloadTaskStatus.Finalizing
            ];
            while (true)
            {
                IEnumerable<DownloadTask> pendingTasks = Tasks.Where(x => pendingStatuses.Contains(x.Status));
                int freeSlots = maxTaskNumber - pendingTasks.Count();
                if (freeSlots > 0)
                {
                    IEnumerable<DownloadTask> queuedTasks = Tasks.Where(x => x.Status == DownloadTaskStatus.Queued).OrderBy(x => x.CreateDate).Take(freeSlots);
                    foreach (DownloadTask queuedTask in queuedTasks)
                    {
                        queuedTask.Start();
                    }
                }
            }
        }

        #endregion
    }
}
