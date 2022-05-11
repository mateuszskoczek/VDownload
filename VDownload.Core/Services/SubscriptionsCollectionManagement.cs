using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Exceptions;
using Windows.Storage;

namespace VDownload.Core.Services
{
    public static class SubscriptionsCollectionManagement
    {
        #region CONSTANTS

        private static readonly StorageFolder SubscriptionFolderLocation = ApplicationData.Current.LocalFolder;
        private static readonly string SubscriptionsFolderName = "Subscriptions";
        private static readonly string SubscriptionFileExtension = "vsub";

        #endregion



        #region PUBLIC METHODS

        public static async Task<(Subscription Subscription, StorageFile SubscriptionFile)[]> GetSubscriptionsAsync()
        {
            List<(Subscription Subscription, StorageFile SubscriptionFile)> subscriptions = new List<(Subscription Subscription,StorageFile SubscriptionFile)> ();
            StorageFolder subscriptionsFolder = await SubscriptionFolderLocation.CreateFolderAsync(SubscriptionsFolderName, CreationCollisionOption.OpenIfExists);
            BinaryFormatter formatter = new BinaryFormatter();
            foreach (StorageFile file in await subscriptionsFolder.GetFilesAsync())
            {
                if (file.Name.EndsWith(SubscriptionFileExtension))
                {
                    Stream fileStream = await file.OpenStreamForReadAsync();
                    Subscription subscription = (Subscription)formatter.Deserialize(fileStream);
                    subscriptions.Add((subscription, file));
                }
            }
            return subscriptions.ToArray();
        }

        public static async Task<StorageFile> CreateSubscriptionFileAsync(Subscription subscription)
        {
            StorageFolder subscriptionsFolder = await SubscriptionFolderLocation.CreateFolderAsync(SubscriptionsFolderName, CreationCollisionOption.OpenIfExists);
            try
            {
                StorageFile subscriptionFile = await subscriptionsFolder.CreateFileAsync($"{(int)subscription.Playlist.Source}-{subscription.Playlist.ID}.{SubscriptionFileExtension}", CreationCollisionOption.FailIfExists);
                BinaryFormatter formatter = new BinaryFormatter();
                Stream subscriptionFileStream = await subscriptionFile.OpenStreamForWriteAsync();
                formatter.Serialize(subscriptionFileStream, subscription);
                return subscriptionFile;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x800700B7)
                {
                    throw new SubscriptionExistsException($"Subscription with id \"{(int)subscription.Playlist.Source}-{subscription.Playlist.ID}\" already exists");
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task UpdateSubscriptionFileAsync(Subscription subscription, StorageFile subscriptionFile)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream subscriptionFileStream = await subscriptionFile.OpenStreamForWriteAsync();
            formatter.Serialize(subscriptionFileStream, subscription);
        }

        public static async Task DeleteSubscriptionFileAsync(StorageFile subscriptionFile) => await subscriptionFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

        #endregion
    }
}
