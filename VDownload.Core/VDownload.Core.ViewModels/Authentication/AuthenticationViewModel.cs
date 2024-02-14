using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Services.Data.Configuration;
using VDownload.Services.UI.Dialogs;
using VDownload.Services.UI.StringResources;
using VDownload.Services.UI.WebView;
using VDownload.Sources.Twitch.Authentication;

namespace VDownload.Core.ViewModels.Authentication
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

        protected readonly IDialogsService _dialogsService;
        protected readonly IWebViewService _webViewService;
        protected readonly IConfigurationService _configurationService;
        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly ITwitchAuthenticationService _twitchAuthenticationService;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        private AuthenticationButton _twitchButtonState = AuthenticationButton.Loading;

        [ObservableProperty]
        private string _twitchDescription;

        #endregion



        #region CONSTRUCTORS

        public AuthenticationViewModel(IDialogsService dialogsService, IWebViewService webViewService, IConfigurationService configurationService, IStringResourcesService stringResourcesService, ITwitchAuthenticationService twitchAuthenticationService)
        {
            _dialogsService = dialogsService;
            _webViewService = webViewService;
            _configurationService = configurationService;
            _stringResourcesService = stringResourcesService;
            _twitchAuthenticationService = twitchAuthenticationService;
        }

        #endregion



        #region PUBLIC METHODS

        [RelayCommand]
        public async Task Navigation()
        {
            List<Task> refreshTasks = new List<Task>
            {
                TwitchAuthenticationRefresh()
            };
            await Task.WhenAll(refreshTasks);
        }

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
                Sources.Twitch.Configuration.Models.Authentication auth = _configurationService.Twitch.Authentication;
                string authUrl = string.Format(auth.Url, auth.ClientId, auth.RedirectUrl, auth.ResponseType, string.Join(' ', auth.Scopes));

                string url = await _webViewService.Show(new Uri(authUrl), (url) => url.StartsWith(auth.RedirectUrl), _stringResourcesService.AuthenticationViewResources.Get("TwitchAuthenticationWindowTitle"));

                Regex regex = new Regex(auth.RedirectUrlRegex);
                Match match = regex.Match(url);

                if (match.Success)
                {
                    string token = match.Groups[1].Value;
                    await _twitchAuthenticationService.SetToken(Encoding.UTF8.GetBytes(token));
                }
                else
                {
                    await _dialogsService.ShowOk(_stringResourcesService.AuthenticationViewResources.Get("TwitchAuthenticationDialogTitle"), "An unknown error occured"); // TODO : Change to string resource
                }
            }
            await TwitchAuthenticationRefresh();
        }

        #endregion



        #region PRIVATE METHODS

        private async Task TwitchAuthenticationRefresh()
        {
            TwitchButtonState = AuthenticationButton.Loading;

            byte[]? token = await _twitchAuthenticationService.GetToken();

            if (token is null)
            {
                TwitchDescription = _stringResourcesService.AuthenticationViewResources.Get("TwitchAuthenticationDescriptionNotAuthenticated");
                TwitchButtonState = AuthenticationButton.SignIn;
            }
            else
            {
                TwitchValidationResult validationResult = await _twitchAuthenticationService.ValidateToken(token);
                if (validationResult.Success)
                {
                    TwitchDescription = string.Format(_stringResourcesService.AuthenticationViewResources.Get("TwitchAuthenticationDescriptionAuthenticated"), validationResult.TokenData.Login, validationResult.TokenData.ExpirationDate);
                    TwitchButtonState = AuthenticationButton.SignOut;
                }
                else
                {
                    await _twitchAuthenticationService.DeleteToken();
                    TwitchDescription = _stringResourcesService.AuthenticationViewResources.Get("TwitchAuthenticationDescriptionAuthenticationInvalid");
                    TwitchButtonState = AuthenticationButton.SignIn;
                }
            }
        }

        #endregion
    }
}
