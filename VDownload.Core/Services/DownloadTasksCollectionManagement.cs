using Microsoft.Toolkit.Uwp.Connectivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Structs;

namespace VDownload.Core.Services
{
    public static class DownloadTasksCollectionManagement
    {
        #region PROPERTIES

        private static readonly Dictionary<string, DownloadTask> ChangeableDownloadTasksCollection = new Dictionary<string, DownloadTask>();
        public static readonly ReadOnlyDictionary<string, DownloadTask> DownloadTasksCollection = new ReadOnlyDictionary<string, DownloadTask>(ChangeableDownloadTasksCollection);

        #endregion



        #region PUBLIC METHODS

        public static string GenerateID()
        {
            char[] idChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            int idLength = 10;

            string id;
            do
            {
                id = "";
                while (id.Length < idLength)
                {
                    id += idChars[new Random().Next(0, idChars.Length)];
                }
            } while (DownloadTasksCollection.Keys.Contains(id));

            return id;
        }

        public static void Add(DownloadTask downloadTask, string id)
        {
            ChangeableDownloadTasksCollection[id] = downloadTask;
        }

        public static void Remove(string id)
        {
            ChangeableDownloadTasksCollection.Remove(id);
        }

        public static async Task WaitInQueue(bool delayWhenOnMeteredConnection, CancellationToken cancellationToken = default)
        {
            while ((ChangeableDownloadTasksCollection.Values.Where((DownloadTask task) => task.Status == Enums.DownloadTaskStatus.Downloading || task.Status == Enums.DownloadTaskStatus.Processing || task.Status == Enums.DownloadTaskStatus.Finalizing).Count() >= (int)Config.GetValue("max_active_video_task") || (delayWhenOnMeteredConnection && NetworkHelper.Instance.ConnectionInformation.IsInternetOnMeteredConnection)) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        #endregion
    }
}
