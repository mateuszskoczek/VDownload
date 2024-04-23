using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SimpleToolkit.UI.WinUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Common;

namespace VDownload.Services.UI.Notifications
{
    public interface INotificationsService : IInitializableService<Window>
    {
        void SendNotification(string title, IEnumerable<string> message);
    }



    public class NotificationsService : INotificationsService
    {
        #region FIELDS

        protected Window _window;

        #endregion



        #region CONSTRUCTORS

        ~NotificationsService()
        {
            AppNotificationManager.Default.Unregister();
        }

        #endregion



        #region PUBLIC METHODS

        public async Task Initialize(Window window) => await Task.Run(() =>
        {
            _window = window;

            AppNotificationManager.Default.NotificationInvoked += NotificationInvoked;

            AppNotificationManager.Default.Register();
        });

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



        #region PRIVATE METHODS

        private void NotificationInvoked(AppNotificationManager sender, AppNotificationActivatedEventArgs args) => WindowHelper.ShowWindow(_window);

        #endregion
    }
}
