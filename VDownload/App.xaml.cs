// Internal
using VDownload.Core.Services;

// System
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

namespace VDownload
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Log.AddHeader("APP LAUNCHED");
            Log.Break();

            // Rebuild configuration file
            Log.AddHeader("REBUILDING CONFIGURATION FILE");
            
            Config.Rebuild();

            Log.Add("Configuration file rebuilded successfully");
            Log.Break();


            // Delete temp on start
            // TODO
            Debug.WriteLine(Config.GetValue("delete_temp_on_start"));
            if ((bool)Config.GetValue("delete_temp_on_start"))
            {
                Log.AddHeader("DELETING TEMPORARY FILES");
                IReadOnlyList<IStorageItem> tempItems = await ApplicationData.Current.TemporaryFolder.GetItemsAsync();
                List<Task> tasks = new List<Task>();
                foreach (IStorageItem item in tempItems) tasks.Add(item.DeleteAsync().AsTask());
                await Task.WhenAll(tasks);
                Log.Add("Temporary files deleted successfully");
                Log.Break();
            }

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (!(Window.Current.Content is Frame rootFrame))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(GUI.Views.MainPage), e.Arguments);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
