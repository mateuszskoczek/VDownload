using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;

namespace VDownload.Views.Sources
{
    public sealed partial class SourcesMain : Page
    {
        #region CONSTRUCTORS

        public SourcesMain()
        {
            InitializeComponent();
        }

        #endregion



        #region MAIN EVENT HANDLERS VOIDS

        // ON NAVIGATED TO THIS PAGE (Check all services)
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Task[] checkingTasks = new Task[1];

            checkingTasks[0] = CheckTwitch();

            await Task.WhenAll(checkingTasks);
        }

        #endregion



        #region TWITCH

        // CHECK TWITCH LOGIN AT START
        private async Task CheckTwitch()
        {
            try
            {
                string twitchAccessToken = await Core.Services.Sources.Twitch.Helpers.Auth.ReadAccessTokenAsync();
                #pragma warning disable IDE0042 // Deconstruct variable declaration
                (bool IsValid, string Login, DateTime? ExpirationDate) twitchAccessTokenValidation = await Core.Services.Sources.Twitch.Helpers.Auth.ValidateAccessTokenAsync(twitchAccessToken);
                #pragma warning restore IDE0042 // Deconstruct variable declaration
                if (twitchAccessTokenValidation.IsValid)
                {
                    SourcesTwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionLoggedIn")} {twitchAccessTokenValidation.Login}";
                    SourcesTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextLoggedIn");
                    SourcesTwitchLoginButton.IsEnabled = true;
                }
                else
                {
                    if (twitchAccessToken != null) await Core.Services.Sources.Twitch.Helpers.Auth.DeleteAccessTokenAsync();
                    SourcesTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionNotLoggedIn");
                    SourcesTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextNotLoggedIn");
                    SourcesTwitchLoginButton.IsEnabled = true;
                }
            }
            catch (WebException)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    SourcesTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionInternetConnectionError");
                    SourcesTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextNotLoggedIn");
                    SourcesTwitchLoginButton.IsEnabled = false;
                }
                else throw;
            }
        }

        // TWITCH LOGIN BUTTON CLICKED
        private async void SourcesTwitchLoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string accessToken = await Core.Services.Sources.Twitch.Helpers.Auth.ReadAccessTokenAsync();
                var accessTokenValidation = await Core.Services.Sources.Twitch.Helpers.Auth.ValidateAccessTokenAsync(accessToken);
                if (accessTokenValidation.IsValid)
                {
                    // Revoke access token
                    await Core.Services.Sources.Twitch.Helpers.Auth.RevokeAccessTokenAsync(accessToken);

                    // Delete access token
                    await Core.Services.Sources.Twitch.Helpers.Auth.DeleteAccessTokenAsync();

                    // Update Twitch SettingControl
                    SourcesTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionNotLoggedIn");
                    SourcesTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextNotLoggedIn");
                }
                else
                {
                    // Open new window
                    AppWindow TwitchAuthWindow = await AppWindow.TryCreateAsync();
                    TwitchAuthWindow.Title = "Twitch Authentication";

                    #pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                    WebView2 TwitchAuthWebView = new WebView2();
                    await TwitchAuthWebView.EnsureCoreWebView2Async();
                    TwitchAuthWebView.Source = Core.Services.Sources.Twitch.Helpers.Auth.AuthorizationUrl;
                    ElementCompositionPreview.SetAppWindowContent(TwitchAuthWindow, TwitchAuthWebView);

                    // NavigationStarting event (only when redirected)
                    TwitchAuthWebView.NavigationStarting += async (s, a) =>
                    {
                        if (new Uri(a.Uri).Host == Core.Services.Sources.Twitch.Helpers.Auth.RedirectUrl.Host)
                        {
                            // Close window
                            await TwitchAuthWindow.CloseAsync();

                            // Get response
                            string response = a.Uri.Replace(Core.Services.Sources.Twitch.Helpers.Auth.RedirectUrl.OriginalString, "");

                            if (response[1] == '#')
                            {
                                // Get access token
                                accessToken = response.Split('&')[0].Replace("/#access_token=", "");

                                // Check token
                                accessTokenValidation = await Core.Services.Sources.Twitch.Helpers.Auth.ValidateAccessTokenAsync(accessToken);

                                // Save token
                                await Core.Services.Sources.Twitch.Helpers.Auth.SaveAccessTokenAsync(accessToken);

                                // Update Twitch SettingControl
                                SourcesTwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionLoggedIn")} {accessTokenValidation.Login}";
                                SourcesTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextLoggedIn");
                            }
                            else
                            {
                                // Ignored errors
                                string[] ignoredErrors = new[]
                                {
                                    "The user denied you access",
                                };

                                // Errors translation
                                Dictionary<string, string> errorsTranslation = new Dictionary<string, string>
                                {

                                };

                                // Get error info
                                string errorInfo = (response.Split('&')[1].Replace("error_description=", "")).Replace('+', ' ');
                                if (!ignoredErrors.Contains(errorInfo))
                                {
                                    // Error
                                    ContentDialog loginErrorDialog = new ContentDialog
                                    {
                                        Title = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginErrorDialogTitle"),
                                        Content = errorsTranslation.Keys.Contains(errorInfo) ? errorsTranslation[errorInfo] : $"{ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginErrorDialogDescriptionUnknown")} ({errorInfo})",
                                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                                    };
                                    await loginErrorDialog.ShowAsync();
                                }
                            }

                            await TwitchAuthWindow.TryShowAsync();

                            // Clear cache 
                            TwitchAuthWebView.CoreWebView2.CookieManager.DeleteAllCookies();
                        }
                    };
                    #pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
                }
            }
            catch (WebException wex)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    SourcesTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionInternetConnectionError");
                    SourcesTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextNotLoggedIn");
                    SourcesTwitchLoginButton.IsEnabled = false;
                    ContentDialog internetAccessErrorDialog = new ContentDialog
                    {
                        Title = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginErrorDialogTitle"),
                        Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginErrorDialogDescriptionInternetConnectionError"),
                        CloseButtonText = ResourceLoader.GetForCurrentView().GetString("CloseErrorDialogButtonText"),
                    };
                    await internetAccessErrorDialog.ShowAsync();
                }
                else throw;
            }
        }

        #endregion
    }
}
