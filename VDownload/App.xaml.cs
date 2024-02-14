using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.Net.Http;
using VDownload.Core.Tasks;
using VDownload.Core.ViewModels;
using VDownload.Core.ViewModels.Authentication;
using VDownload.Core.ViewModels.Home;
using VDownload.Core.ViewModels.Settings;
using VDownload.Core.Views;
using VDownload.Core.Views.Authentication;
using VDownload.Core.Views.Home;
using VDownload.Core.Views.Settings;
using VDownload.Services.Data.Authentication;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.DictionaryResources;
using VDownload.Services.UI.Notifications;
using VDownload.Services.UI.StoragePicker;
using VDownload.Services.UI.StringResources;
using VDownload.Services.UI.WebView;
using VDownload.Services.Utility.Encryption;
using VDownload.Services.Utility.FFmpeg;
using VDownload.Services.Utility.HttpClient;
using VDownload.Sources;
using VDownload.Sources.Twitch;
using VDownload.Sources.Twitch.Api;
using VDownload.Sources.Twitch.Authentication;
using Windows.Graphics.Printing;

namespace VDownload
{
    public partial class App : Application
    {
        #region FIELDS

        protected IServiceProvider _serviceProvider;

        protected BaseWindow _window;

        #endregion



        #region CONSTRUCTORS

        public App()
        {
            this.InitializeComponent();


            IServiceCollection services = new ServiceCollection();

            BuildCore(services);

            BuildConfiguration(services);

            BuildDataServices(services);
            BuildUIServices(services);
            BuildUtilityServices(services);
            BuildSourcesServices(services);

            BuildTasksManager(services);
            BuildPresentation(services);

            _serviceProvider = services.BuildServiceProvider();
        }

        #endregion



        #region PRIVATE METHODS

        protected void BuildCore(IServiceCollection services)
        {
            services.AddSingleton<HttpClient>();
        }

        protected void BuildConfiguration(IServiceCollection services)
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder
            {
                Sources =
                {
                    new JsonConfigurationSource
                    {
                        Path = "configuration.json"
                    }
                }
            };
            IConfiguration config = configBuilder.Build();
            services.AddSingleton(config);
        }

        protected void BuildDataServices(IServiceCollection services)
        {
            services.AddSingleton<IConfigurationService, ConfigurationService>();
            services.AddSingleton<IAuthenticationDataService, AuthenticationDataService>();
            services.AddSingleton<ISettingsService, SettingsService>();
        }

        protected void BuildUIServices(IServiceCollection services)
        {
            services.AddSingleton<IWebViewService, WebViewService>();
            services.AddSingleton<IStoragePickerService, StoragePickerService>();
            services.AddSingleton<IDialogsService, DialogsService>();
            services.AddSingleton<INotificationsService, NotificationsService>();
            services.AddSingleton<IStringResourcesService, StringResourcesService>();
            services.AddSingleton<IDictionaryResourcesService, DictionaryResourcesService>();
        }

        protected void BuildUtilityServices(IServiceCollection services)
        {
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IFFmpegService, FFmpegService>();
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
            services.AddSingleton<AuthenticationViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<HomeDownloadsViewModel>();
            services.AddSingleton<HomeVideoViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<BaseViewModel>();

            // Views
            services.AddTransient<AuthenticationView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<HomeDownloadsView>();
            services.AddTransient<HomeVideoView>();
            services.AddTransient<HomeView>();
            services.AddTransient<BaseWindow>();
        }

        protected void AssignStaticProperties()
        {
            IStoragePickerService storagePickerService = _serviceProvider.GetService<IStoragePickerService>();
            storagePickerService.DefaultRoot = _window;

            ViewModelToViewConverter.ServiceProvider = _serviceProvider;
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            _window = _serviceProvider.GetService<BaseWindow>();
            _window.RootLoaded += Window_RootLoaded;
            _window.Activate();

            AssignStaticProperties();
        }

        protected void Window_RootLoaded(object sender, EventArgs e)
        {
            IDialogsService dialogsService = _serviceProvider.GetService<IDialogsService>();
            dialogsService.DefaultRoot = _window.XamlRoot;
        }

        #endregion
    }
}
