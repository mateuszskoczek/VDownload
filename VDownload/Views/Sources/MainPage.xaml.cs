using Microsoft.Toolkit.Uwp.Connectivity;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Structs;
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

        private async void TwitchSettingControlLoginButton_Click(object sender, RoutedEventArgs e)
        {
            async Task ShowDialog(string localErrorKey, string unknownErrorInfo = null)
            {
                StringBuilder content = new StringBuilder(ResourceLoader.GetForCurrentView("DialogResources").GetString($"Sources_TwitchLogin_{localErrorKey}_Content"));
                if (!string.IsNullOrEmpty(unknownErrorInfo))
                {
                    content.Append($" {unknownErrorInfo}");
                }
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = ResourceLoader.GetForCurrentView("DialogResources").GetString("Sources_TwitchLogin_Base_Title"),
                    Content = content.ToString(),
                    CloseButtonText = ResourceLoader.GetForCurrentView().GetString("Base_CloseButtonText"),
                };
                await errorDialog.ShowAsync();
            }

            TwitchAccessTokenValidationData accessTokenValidation = TwitchAccessTokenValidationData.Null;
            try
            {
                string accessToken = await Core.Services.Sources.Twitch.Helpers.Authorization.ReadAccessTokenAsync();
                accessTokenValidation = await Core.Services.Sources.Twitch.Helpers.Authorization.ValidateAccessTokenAsync(accessToken);
            }
            catch (WebException)
            {
                if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                {
                    goto Check;
                }
                else throw;
            }

            if (accessTokenValidation.IsValid)
            {
                await Core.Services.Sources.Twitch.Helpers.Authorization.RevokeAccessTokenAsync(accessTokenValidation.AccessToken);
                await Core.Services.Sources.Twitch.Helpers.Authorization.DeleteAccessTokenAsync();
            }
            else
            {
                string response = await LoginToTwitch();
                accessTokenValidation = TwitchAccessTokenValidationData.Null;
                try
                {
                    accessTokenValidation = await Core.Services.Sources.Twitch.Helpers.Authorization.ValidateAccessTokenAsync(response);
                }
                catch (WebException)
                {
                    if (!NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable)
                    {
                        await ShowDialog("InternetNotAvailable");
                        goto Check;
                    }
                    else throw;
                }
                if (accessTokenValidation.IsValid)
                {
                    await Core.Services.Sources.Twitch.Helpers.Authorization.SaveAccessTokenAsync(accessTokenValidation.AccessToken);
                }
                else
                {
                    Dictionary<string, string> errorCodes = new Dictionary<string, string>
                    {
                        {"The user denied you access", string.Empty},
                    };

                    if (!string.IsNullOrEmpty(response))
                    {
                        if (errorCodes.ContainsKey(response))
                        {
                            if (!string.IsNullOrEmpty(errorCodes[response]))
                            {
                                await ShowDialog(errorCodes[response]);
                            }
                        }
                        else
                        {
                            await ShowDialog("Unknown", response);
                        }
                    }
                }
            }
            Check: await CheckTwitch();
        }

        #endregion



        #region PRIVATE METHODS

        private async Task CheckTwitch()
        {
            try
            {
                string twitchAccessToken = await Core.Services.Sources.Twitch.Helpers.Authorization.ReadAccessTokenAsync();
                TwitchAccessTokenValidationData twitchAccessTokenValidation = await Core.Services.Sources.Twitch.Helpers.Authorization.ValidateAccessTokenAsync(twitchAccessToken);
                if (twitchAccessTokenValidation.IsValid)
                {
                    TwitchSettingControl.Description = $"{ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_LoggedIn")} {twitchAccessTokenValidation.Login}";
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_LoggedIn");
                }
                else
                {
                    if (twitchAccessToken != null)
                    {
                        TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_InvalidAccessToken");
                    }
                    else
                    {
                        TwitchSettingControl.Description = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_Description_NotLoggedIn");
                    }
                    TwitchSettingControlLoginButton.Content = ResourceLoader.GetForCurrentView().GetString("Sources_TwitchSettingControl_LoginButton_Content_NotLoggedIn");
                }
                TwitchSettingControlLoginButton.IsEnabled = true;
            }
            catch (WebException)
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

        private async Task<string> LoginToTwitch()
        {
            string response = string.Empty;
            AppWindow TwitchAuthWindow = await AppWindow.TryCreateAsync();
            TwitchAuthWindow.Title = "Twitch Authentication";

#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            WebView2 TwitchAuthWebView = new WebView2();
            await TwitchAuthWebView.EnsureCoreWebView2Async();
            TwitchAuthWebView.Source = Core.Services.Sources.Twitch.Helpers.Authorization.AuthorizationUrl;

            ElementCompositionPreview.SetAppWindowContent(TwitchAuthWindow, TwitchAuthWebView);

            TwitchAuthWebView.NavigationStarting += async (s, a) =>
            {
                if (new Uri(a.Uri).Host == Core.Services.Sources.Twitch.Helpers.Authorization.RedirectUrl.Host)
                {
                    string login_response = a.Uri.Replace(Core.Services.Sources.Twitch.Helpers.Authorization.RedirectUrl.OriginalString, "");

                    if (login_response[1] == '#')
                    {
                        response = login_response.Split('&')[0].Replace("/#access_token=", "");
                    }
                    else
                    {
                        response = (login_response.Split('&')[1].Replace("error_description=", "")).Replace('+', ' ');
                    }

                    await TwitchAuthWindow.CloseAsync();
                }
            };

            await TwitchAuthWindow.TryShowAsync();

            try
            {
                while (TwitchAuthWindow.IsVisible)
                {
                    await Task.Delay(1);
                }
            }
            catch (ObjectDisposedException) { }

            TwitchAuthWebView.CoreWebView2.CookieManager.DeleteAllCookies();
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.

            return response;
        }

        #endregion
    }
}
