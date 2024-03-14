using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Views;
using VDownload.Services.Common;
using VDownload.Services.Data.Application;
using VDownload.Services.Data.Authentication;
using VDownload.Services.Data.Settings;
using VDownload.Services.Data.Subscriptions;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.DictionaryResources;
using VDownload.Services.UI.Notifications;
using VDownload.Services.UI.StoragePicker;

namespace VDownload.Activation
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }



    public class ActivationService : IActivationService
    {
        #region SERVICES
        protected readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
        protected readonly IEnumerable<IActivationHandler> _activationHandlers;

        #endregion



        #region FIELDS

        protected readonly ICollection<Func<Task>> _beforeActivationInitializations = new List<Func<Task>>();
        protected readonly ICollection<Func<Task>> _afterActivationInitializations = new List<Func<Task>>();
        protected readonly ICollection<Func<Task>> _afterWindowRootLoadedInitializations = new List<Func<Task>>();

        protected BaseWindow _window;

        #endregion



        #region CONSTRUCTORS

        public ActivationService(ActivationHandler<LaunchActivatedEventArgs> defaultHandler, IEnumerable<IActivationHandler> activationHandlers, ISettingsService settingsService, IApplicationDataService applicationDataService, IAuthenticationDataService authenticationDataService, ISubscriptionsDataService subscriptionsDataService, IStoragePickerService storagePickerService, INotificationsService notificationsService, IDictionaryResourcesService dictionaryResourcesService, IDialogsService dialogsService)
        {
            _defaultHandler = defaultHandler;
            _activationHandlers = activationHandlers;

            _beforeActivationInitializations.Add(() => dictionaryResourcesService.Initialize((App.Current as App).Resources));

            _afterActivationInitializations.Add(settingsService.Initialize);
            _afterActivationInitializations.Add(applicationDataService.Initialize);
            _afterActivationInitializations.Add(authenticationDataService.Initialize);
            _afterActivationInitializations.Add(subscriptionsDataService.Initialize);
            _afterActivationInitializations.Add(() => storagePickerService.Initialize(_window));
            _afterActivationInitializations.Add(() => notificationsService.Initialize(_window));

            _afterWindowRootLoadedInitializations.Add(() => dialogsService.Initialize(_window.XamlRoot));
        }

        #endregion



        #region PUBLIC METHODS

        public async Task ActivateAsync(object activationArgs)
        {
            await InitializeAsync();
            ViewModelToViewConverter.Initialize((App.Current as App)!.Host.Services);

            await HandleActivationAsync(activationArgs);

            _window = App.GetService<BaseWindow>();
            _window.RootLoaded += Window_RootLoaded;
            _window.Activate();

            await StartupAsync();
        }

        #endregion



        #region PRIVATE METHODS

        #region EVENT HANDLERS

        protected async void Window_RootLoaded(object sender, EventArgs e) => await AfterWindowRootLoaded();

        #endregion

        protected async Task InitializeAsync()
        {
            List<Task> tasks = new List<Task>();
            foreach (Func<Task> init in _beforeActivationInitializations)
            {
                tasks.Add(init.Invoke());
            }
            await Task.WhenAll(tasks);
        }

        protected async Task StartupAsync() => await Task.WhenAll(_afterActivationInitializations.Select(x => x.Invoke()));

        protected async Task AfterWindowRootLoaded() => await Task.WhenAll(_afterWindowRootLoadedInitializations.Select(x => x.Invoke()));

        protected async Task HandleActivationAsync(object activationArgs)
        {
            var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

            if (activationHandler != null)
            {
                await activationHandler.HandleAsync(activationArgs);
            }

            if (_defaultHandler.CanHandle(activationArgs))
            {
                await _defaultHandler.HandleAsync(activationArgs);
            }
        }

        #endregion
    }
}
