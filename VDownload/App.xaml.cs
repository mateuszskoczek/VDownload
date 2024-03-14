using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using VDownload.Activation;
using VDownload.Core.Tasks;
using VDownload.Core.ViewModels;
using VDownload.Core.ViewModels.About;
using VDownload.Core.ViewModels.Authentication;
using VDownload.Core.ViewModels.Home;
using VDownload.Core.ViewModels.Settings;
using VDownload.Core.ViewModels.Subscriptions;
using VDownload.Core.Views;
using VDownload.Core.Views.About;
using VDownload.Core.Views.Authentication;
using VDownload.Core.Views.Home;
using VDownload.Core.Views.Settings;
using VDownload.Core.Views.Subscriptions;
using VDownload.Services.Data.Application;
using VDownload.Services.Data.Authentication;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Configuration.Models;
using VDownload.Services.Data.Settings;
using VDownload.Services.Data.Subscriptions;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.DictionaryResources;
using VDownload.Services.UI.Notifications;
using VDownload.Services.UI.StoragePicker;
using VDownload.Services.UI.StringResources;
using VDownload.Services.UI.WebView;
using VDownload.Services.Utility.Encryption;
using VDownload.Services.Utility.FFmpeg;
using VDownload.Services.Utility.Filename;
using VDownload.Services.Utility.HttpClient;
using VDownload.Sources;
using VDownload.Sources.Twitch;
using VDownload.Sources.Twitch.Api;
using VDownload.Sources.Twitch.Authentication;
using Windows.Graphics.Printing;
using Windows.UI.Notifications;

namespace VDownload
{
    public partial class App : Application
    {
        #region PROPERTIES

        public static BaseWindow Window { get; protected set; }

        public static T GetService<T>() where T : class
        {
            if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }

        public IHost Host { get; set; }

        #endregion



        #region CONSTRUCTORS

        public App()
        {
            this.InitializeComponent();

            Host = Microsoft.Extensions.Hosting.Host
                   .CreateDefaultBuilder()
                   .UseContentRoot(AppContext.BaseDirectory)
                   .ConfigureAppConfiguration((builder) =>
                   {
                       builder.Sources.Add(new JsonConfigurationSource
                       {
                           Path = "configuration.json"
                       });
                   })
                   .ConfigureServices((context, services) =>
                   {
                       BuildCore(services);

                       BuildDataServices(services);
                       BuildUIServices(services);
                       BuildUtilityServices(services);
                       BuildSourcesServices(services);

                       BuildTasksManager(services);
                       BuildPresentation(services);
                       BuildActivation(services);
                   })
                   .Build();

            UnhandledException += UnhandledExceptionCatched;
        }

        #endregion



        #region PRIVATE METHODS

        #region EVENT HANDLERS

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            await GetService<IActivationService>().ActivateAsync(args);
        }

        protected void UnhandledExceptionCatched(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        protected void BuildCore(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
        }

        protected void BuildDataServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IAuthenticationDataService, AuthenticationDataService>();
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IApplicationDataService, ApplicationDataService>();
            services.AddSingleton<ISubscriptionsDataService, SubscriptionsDataService>();
        }

        protected void BuildUIServices(IServiceCollection services)
        {
            services.AddSingleton<IStringResourcesService, StringResourcesService>();
            services.AddSingleton<IDictionaryResourcesService, DictionaryResourcesService>();
            services.AddSingleton<IWebViewService, WebViewService>();
            services.AddSingleton<IStoragePickerService, StoragePickerService>();
            services.AddSingleton<INotificationsService, NotificationsService>();
            services.AddSingleton<IDialogsService, DialogsService>();
        }

        protected void BuildUtilityServices(IServiceCollection services)
        {
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IFFmpegService, FFmpegService>();
            services.AddSingleton<IFilenameService, FilenameService>();
        }

        protected void BuildSourcesServices(IServiceCollection services)
        {
            // Twitch
            services.AddSingleton<ITwitchApiService, TwitchApiService>();
            services.AddSingleton<ITwitchAuthenticationService, TwitchAuthenticationService>();
            services.AddSingleton<ITwitchVideoStreamFactoryService, TwitchVideoStreamFactoryService>();
            services.AddSingleton<ITwitchSearchService, TwitchSearchService>();

            // Base
            services.AddSingleton<ISearchService, SearchService>();
        }

        protected void BuildTasksManager(IServiceCollection services)
        {
            services.AddSingleton<IDownloadTaskFactoryService, DownloadTaskFactoryService>();
            services.AddSingleton<IDownloadTaskManager, DownloadTaskManager>();
        }

        protected void BuildPresentation(IServiceCollection services)
        {
            // ViewModels
            services.AddSingleton<AboutViewModel>();
            services.AddSingleton<AuthenticationViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HomeDownloadsViewModel>();
            services.AddSingleton<HomeVideoViewModel>();
            services.AddSingleton<HomeVideoCollectionViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<SubscriptionsViewModel>();
            services.AddSingleton<BaseViewModel>();

            // Views
            services.AddTransient<AboutView>();
            services.AddTransient<AuthenticationView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<HomeDownloadsView>();
            services.AddTransient<HomeVideoView>();
            services.AddTransient<HomeVideoCollectionView>();
            services.AddTransient<HomeView>();
            services.AddTransient<SubscriptionsView>();
            services.AddTransient<BaseWindow>();
        }

        protected void BuildActivation(IServiceCollection services)
        {
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            services.AddSingleton<IActivationService, ActivationService>();
        }

        #endregion
    }
}
