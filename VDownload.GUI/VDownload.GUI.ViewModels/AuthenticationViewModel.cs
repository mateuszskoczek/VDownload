using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.GUI.Services.Dialog;
using VDownload.GUI.Services.WebView;
using VDownload.Sources.Twitch;
using VDownload.Sources.Twitch.Authentication;

namespace VDownload.GUI.ViewModels
{
    public partial class AuthenticationViewModel : ObservableObject
    {
        #region ENUMS

        public enum AuthenticationButton
        {
            SignIn,
            SignOut,
            Loading
        }

        #endregion



        #region SERVICES

        private IDialogService _dialogService;
        private IWebViewService _webViewService;
        private ITwitchAuthenticationService _twitchAuthenticationService;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private string _twitchDescription;

        [ObservableProperty]
        private AuthenticationButton _twitchButtonState;

        #endregion



        #region CONSTRUCTORS

        public AuthenticationViewModel(IDialogService dialogService, IWebViewService webViewService, ITwitchAuthenticationService twitchAuthenticationService)
        {
            _dialogService = dialogService;
            _webViewService = webViewService;
            _twitchAuthenticationService = twitchAuthenticationService;

            TwitchButtonState = AuthenticationButton.Loading;
        }

        #endregion



        #region PUBLIC METHODS

        [RelayCommand]
        public async Task TwitchAuthentication()
        {
            AuthenticationButton state = TwitchButtonState;
            TwitchButtonState = AuthenticationButton.Loading;

            if (state == AuthenticationButton.SignOut)
            {
                await _twitchAuthenticationService.DeleteToken();
            }
            else
            {
                string url = await _webViewService.Show(new Uri(_twitchAuthenticationService.AuthenticationPageUrl), _twitchAuthenticationService.AuthenticationPageClosePredicate, "Twitch authentication");

                Match match = _twitchAuthenticationService.AuthenticationPageRedirectUrlRegex.Match(url);

                if (match.Success)
                {
                    string token = match.Groups[1].Value;
                    await _twitchAuthenticationService.SetToken(Encoding.UTF8.GetBytes(token));
                }
                else
                {
                    await _dialogService.ShowOk("Twitch authentication error", "An error occured");
                }
            }
            await TwitchAuthenticationRefresh();
        }

        [RelayCommand]
        public async Task Navigation()
        {
            List<Task> refreshTasks = new List<Task>
            {
                TwitchAuthenticationRefresh() 
            };
            await Task.WhenAll(refreshTasks);
        }

        #endregion



        #region PRIVATE METHODS

        private async Task TwitchAuthenticationRefresh()
        {
            TwitchButtonState = AuthenticationButton.Loading;

            byte[]? token = await _twitchAuthenticationService.GetToken();

            if (token is null)
            {
                TwitchDescription = "You are not authenticated. Please sign in";
                TwitchButtonState = AuthenticationButton.SignIn;
            }
            else
            {
                TwitchValidationResult validationResult = await _twitchAuthenticationService.ValidateToken(token);
                if (validationResult.Success)
                {
                    TwitchDescription = $"Signed in as {validationResult.TokenData.Login}. Expiration date: {validationResult.TokenData.ExpirationDate:dd.MM.yyyy HH:mm}";
                    TwitchButtonState = AuthenticationButton.SignOut;
                }
                else
                {
                    await _twitchAuthenticationService.DeleteToken();
                    TwitchDescription = "Token expired or is invalid. Please log in again";
                    TwitchButtonState = AuthenticationButton.SignIn;
                }
            }
        }

        #endregion
    }
}
