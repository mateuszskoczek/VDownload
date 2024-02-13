using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using VDownload.GUI.Services.Dialog;
using VDownload.GUI.Services.ResourceDictionaries;
using VDownload.GUI.Services.StoragePicker;
using VDownload.GUI.Services.WebView;
using VDownload.GUI.ViewModels;
using VDownload.GUI.Views;
using VDownload.Services.Authentication;
using VDownload.Services.Encryption;
using VDownload.Services.HttpClient;
using VDownload.Services.Search;
using VDownload.Sources.Twitch;
using VDownload.Sources.Twitch.Api;
using VDownload.Sources.Twitch.Authentication;
using VDownload.Sources.Twitch.Configuration;
using VDownload.Sources.Twitch.Search;
using VDownload.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace VDownload
{
    public partial class App : Application
    {
        #region FIELDS

        private MainWindow _window;

        #endregion



        #region CONSTRUCTORS

        public App()
        {
            this.InitializeComponent();
            
            ServiceCollection services = new ServiceCollection();

            // Configuration
            IConfigurationBuilder configBuilder = new ConfigurationBuilder
            {
                Sources =
                {
                    new JsonConfigurationSource
                    {
                        Path = "appsettings.json"
                    }
                }
            };
            IConfiguration config = configBuilder.Build();
            services.AddSingleton(config);

            // Configurations
            services.AddSingleton<AuthenticationConfiguration>();
            services.AddSingleton<TwitchConfiguration>();

            // Http client
            services.AddSingleton<HttpClient>();

            // Services
            services.AddSingleton<IHttpClientService, HttpClientService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ITwitchApiService, TwitchApiService>();
            services.AddSingleton<ITwitchAuthenticationService, TwitchAuthenticationService>();
            services.AddSingleton<ITwitchSearchService, TwitchSearchService>();
            services.AddSingleton<ISearchService, SearchService>();

            // Tasks manager
            services.AddSingleton<IDownloadTasksManager, DownloadTasksManager>();

            // Resource dictionaries
            services.AddSingleton<IImagesResourceDictionary, ImagesResourceDictionary>();

            // GUI Services
            services.AddSingleton<IStoragePickerService, StoragePickerService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWebViewService, WebViewService>();
            services.AddSingleton<IResourceDictionariesServices, ResourceDictionariesServices>();

            // ViewModels
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<AuthenticationViewModel>();

            // Views
            services.AddTransient<HomeView>();
            services.AddTransient<SettingsView>();
            services.AddTransient<AuthenticationView>();

            // Window
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<MainWindow>();

            Services.ServiceProvider.Instance = services.BuildServiceProvider();
        }

        #endregion



        #region PRIVATE METHODS

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = Services.ServiceProvider.Instance.GetService<MainWindow>();
            _window.Activate();

            IDialogService dialogService = Services.ServiceProvider.Instance.GetService<IDialogService>();
            dialogService.DefaultRoot = _window.Content.XamlRoot;

            IStoragePickerService storagePickerService = Services.ServiceProvider.Instance.GetService<IStoragePickerService>();
            storagePickerService.DefaultRoot = _window;
        }

        #endregion
    }
}
