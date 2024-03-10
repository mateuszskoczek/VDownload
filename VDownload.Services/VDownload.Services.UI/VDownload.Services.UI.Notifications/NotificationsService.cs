using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.UI.Notifications
{
    public interface INotificationsService
    {
        void Initialize(Action notificationInvoked);
        void SendNotification(string title, IEnumerable<string> message);
    }



    public class NotificationsService : INotificationsService
    {
        #region CONSTRCUTORS

        ~NotificationsService()
        {
            AppNotificationManager.Default.Unregister();
        }

        #endregion



        #region PUBLIC METHODS

        public void Initialize(Action notificationInvoked)
        {
            AppNotificationManager.Default.NotificationInvoked += (obj, args) => notificationInvoked.Invoke();

            AppNotificationManager.Default.Register();
        }

        public void SendNotification(string title, IEnumerable<string> message)
        {
            AppNotificationBuilder builder = new AppNotificationBuilder();
            builder.AddText(title);
            foreach (string text in message)
            {
                builder.AddText(text);
            }

            AppNotification notification = builder.BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }

        #endregion
    }
}
