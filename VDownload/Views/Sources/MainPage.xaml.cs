using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class MainPage : Page
    {
        #region CONSTRUCTORS

        public MainPage()
        {
            InitializeComponent();
        }

        #endregion



        #region EVENT HANDLERS

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Task[] checkingTasks = new Task[1];
            
            checkingTasks[0] = CheckTwitch();

            await Task.WhenAll(checkingTasks);
        }

        #endregion



        #region PRIVATE METHODS



        #endregion

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
                    TwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_LoggedIn")} {twitchAccessTokenValidation.Login}";
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_LoggedIn");
                }
                else
                {
                    if (twitchAccessToken != null)
                    {
                        TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_AccessTokenExpired");
                    }
                    else if (twitchAccessTokenValidation.ExpirationDate < DateTime.Now)
                    {
                        TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_NotLoggedIn");
                    }
                    Debug.WriteLine(twitchAccessTokenValidation.ExpirationDate.Value.ToString("dd.MM.yyyy"));
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_NotLoggedIn");
                }
                TwitchSettingControlLoginButton.IsEnabled = true;
            }
            catch (WebException ex)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_InternetNotAvailable");
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_NotLoggedIn");
                    TwitchSettingControlLoginButton.IsEnabled = false;
                }
                else throw;
            }
        }

        #region TWITCH



        // TWITCH LOGIN BUTTON CLICKED
        private async void TwitchSettingControlLoginButton_Click(object sender, RoutedEventArgs e)
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
                    TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_NotLoggedIn");
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_NotLoggedIn");
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
                                TwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_LoggedIn")} {accessTokenValidation.Login}";
                                TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_LoggedIn");
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
                        }
                    };
                    #pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.

                    await TwitchAuthWindow.TryShowAsync();

                    // Clear cache 
                    TwitchAuthWebView.CoreWebView2.CookieManager.DeleteAllCookies();
                }
            }
            catch (WebException wex)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchSettingControlDescriptionInternetConnectionError");
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("SourcesTwitchLoginButtonTextNotLoggedIn");
                    TwitchSettingControlLoginButton.IsEnabled = false;
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
