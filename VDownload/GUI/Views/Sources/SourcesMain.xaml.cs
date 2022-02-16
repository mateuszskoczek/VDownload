using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VDownload.GUI.Views.Sources
{
    public sealed partial class SourcesMain : Page
    {
        #region CONSTRUCTORS

        public SourcesMain()
        {
            InitializeComponent();

            // Twitch loading
            SourcesMainTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchSettingControlDescriptionLoading");
            SourcesMainTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginButtonTextLoading");
        }

        #endregion



        #region MAIN

        // ONNAVIGATEDTO EVENT (CHECK SOURCES AUTHENTICATION)
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Check Twitch
            string twitchAccessToken = await Core.Services.Sources.Twitch.Auth.ReadAccessTokenAsync();
            var twitchAccessTokenValidation = await Core.Services.Sources.Twitch.Auth.ValidateAccessTokenAsync(twitchAccessToken);
            if (twitchAccessToken != null && twitchAccessTokenValidation.IsValid)
            {
                Debug.WriteLine("Twitch authentication status: LOGGED_IN");
                Debug.WriteLine(twitchAccessTokenValidation.ExpirationDate.Value.ToString("dd.MM.yyyy"));
                SourcesMainTwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchSettingControlDescriptionLoggedIn")} {twitchAccessTokenValidation.Login}";
                SourcesMainTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginButtonTextLoggedIn");
            }
            else if (twitchAccessToken == null || !twitchAccessTokenValidation.IsValid)
            {
                if (twitchAccessToken != null)
                {
                    Debug.WriteLine("Twitch authentication status: ACCESS_TOKEN_READ_BUT_NOT_VALID");
                    await Core.Services.Sources.Twitch.Auth.DeleteAccessTokenAsync();
                }
                else
                {
                    Debug.WriteLine("Twitch authentication status: ACCESS_TOKEN_NOT_FOUND");
                }
                SourcesMainTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchSettingControlDescriptionNotLoggedIn");
                SourcesMainTwitchSettingControl.DescriptionColor = new SolidColorBrush(Color.FromArgb(255, 225, 0, 0));
                SourcesMainTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginButtonTextNotLoggedIn");
            }
        }

        #endregion



        #region LOGIN BUTTONS

        // TWITCH LOGIN
        private async void SourcesMainTwitchLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string accessToken = await Core.Services.Sources.Twitch.Auth.ReadAccessTokenAsync();
            var accessTokenValidation = await Core.Services.Sources.Twitch.Auth.ValidateAccessTokenAsync(accessToken);
            if (accessToken != null && accessTokenValidation.IsValid)
            {
                Debug.WriteLine("Log out from Twitch (revoke and delete access token)");

                // Revoke access token
                await Core.Services.Sources.Twitch.Auth.RevokeAccessTokenAsync(accessToken);
                Debug.WriteLine($"Twitch access token ({accessToken}) revoked successfully");

                // Delete access token
                await Core.Services.Sources.Twitch.Auth.DeleteAccessTokenAsync();
                Debug.WriteLine($"Twitch access token ({accessToken}) deleted successfully");

                // Update Twitch SettingControl
                Debug.WriteLine("Twitch authentication status: ACCESS_TOKEN_NOT_FOUND");
                SourcesMainTwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchSettingControlDescriptionNotLoggedIn");
                SourcesMainTwitchSettingControl.DescriptionColor = new SolidColorBrush(Color.FromArgb(255, 225, 0, 0));
                SourcesMainTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginButtonTextNotLoggedIn");
            }
            else
            {
                Debug.WriteLine("Log in to Twitch (get, validate and save access token)");

                // Open new window
                AppWindow TwitchAuthWindow = await AppWindow.TryCreateAsync();
                TwitchAuthWindow.Title = "Twitch Authentication";

                WebView2 TwitchAuthWebView = new WebView2();
                await TwitchAuthWebView.EnsureCoreWebView2Async();
                TwitchAuthWebView.Source = Core.Services.Sources.Twitch.Auth.AuthorizationUrl;
                ElementCompositionPreview.SetAppWindowContent(TwitchAuthWindow, TwitchAuthWebView);

                TwitchAuthWindow.TryShowAsync();

                // NavigationStarting event (only when redirected)
                TwitchAuthWebView.NavigationStarting += async (s, a) =>
                {
                    Debug.WriteLine($"TwitchAuthWebView redirected to {a.Uri}");
                    if (new Uri(a.Uri).Host == Core.Services.Sources.Twitch.Auth.RedirectUrl.Host)
                    {
                        // Close window
                        await TwitchAuthWindow.CloseAsync();

                        // Get response
                        string response = a.Uri.Replace(Core.Services.Sources.Twitch.Auth.RedirectUrl.OriginalString, "");

                        if (response[1] == '#')
                        {
                            // Get access token
                            accessToken = response.Split('&')[0].Replace("/#access_token=", "");
                            Debug.WriteLine($"Twitch access token got successfully ({accessToken})");

                            // Check token
                            accessTokenValidation = await Core.Services.Sources.Twitch.Auth.ValidateAccessTokenAsync(accessToken);
                            Debug.WriteLine("Twitch access token validated successfully");

                            // Save token
                            await Core.Services.Sources.Twitch.Auth.SaveAccessTokenAsync(accessToken);
                            Debug.WriteLine("Twitch access token saved successfully");

                            // Update Twitch SettingControl
                            Debug.WriteLine("Twitch authentication status: LOGGED_IN");
                            SourcesMainTwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchSettingControlDescriptionLoggedIn")} {accessTokenValidation.Login}";
                            SourcesMainTwitchSettingControl.DescriptionColor = new SolidColorBrush((Color)Application.Current.Resources["SystemBaseMediumColor"]);
                            SourcesMainTwitchLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginButtonTextLoggedIn");
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
                                    Title = ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginErrorDialogTitle"),
                                    Content = errorsTranslation.Keys.Contains(errorInfo) ? errorsTranslation[errorInfo] : $"{ResourceLoader.GetForCurrentView().GetString("SourcesMainTwitchLoginErrorDialogDescriptionUnknown")} ({errorInfo})",
                                    CloseButtonText = "OK",
                                };
                                await loginErrorDialog.ShowAsync();
                            }
                            Debug.WriteLine($"Log in to Twitch failed ({errorInfo})");
                        }

                        // Clear cache 
                        TwitchAuthWebView.CoreWebView2.CookieManager.DeleteAllCookies();
                    }
                };
            }
        }

        #endregion
    }
}
