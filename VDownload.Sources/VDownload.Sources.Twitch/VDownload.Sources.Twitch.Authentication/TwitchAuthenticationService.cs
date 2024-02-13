using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Services.Authentication;
using VDownload.Services.Encryption;
using VDownload.Services.HttpClient;
using VDownload.Sources.Twitch.Authentication.Models;
using VDownload.Sources.Twitch.Configuration;

namespace VDownload.Sources.Twitch.Authentication
{
    public interface ITwitchAuthenticationService
    {
        #region PROPERTIES

        string AuthenticationPageUrl { get; }
        Regex AuthenticationPageRedirectUrlRegex { get; }


        #endregion



        #region METHODS

        Task<byte[]?> GetToken();
        Task SetToken(byte[] token);
        Task DeleteToken();
        Task<TwitchValidationResult> ValidateToken(byte[] token);
        bool AuthenticationPageClosePredicate(string url);

        #endregion
    }



    public class TwitchAuthenticationService : ITwitchAuthenticationService
    {
        #region SERVICES

        private TwitchAuthenticationConfiguration _authenticationConfiguration;
        private TwitchApiAuthConfiguration _apiAuthConfiguration;

        private IHttpClientService _httpClientService;
        private IAuthenticationService _authenticationService;
        private IEncryptionService _encryptionService;

        #endregion



        #region PROPERTIES

        public string AuthenticationPageUrl { get; private set; }
        public Regex AuthenticationPageRedirectUrlRegex { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public TwitchAuthenticationService(TwitchConfiguration configuration, IHttpClientService httpClientService, IAuthenticationService authenticationService, IEncryptionService encryptionService) 
        {
            _authenticationConfiguration = configuration.Authentication;
            _apiAuthConfiguration = configuration.Api.Auth;

            _httpClientService = httpClientService;
            _authenticationService = authenticationService;
            _encryptionService = encryptionService;

            AuthenticationPageUrl = string.Format(_authenticationConfiguration.Url, _authenticationConfiguration.ClientId, _authenticationConfiguration.RedirectUrl, _authenticationConfiguration.ResponseType, string.Join(' ', _authenticationConfiguration.Scopes));
            AuthenticationPageRedirectUrlRegex = _authenticationConfiguration.RedirectUrlRegex;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<byte[]?> GetToken()
        {
            await _authenticationService.Load();

            byte[]? tokenEncrypted = _authenticationService.AuthenticationData.Twitch.Token;

            if (tokenEncrypted is not null && tokenEncrypted.Length == 0) 
            {
                tokenEncrypted = null;
            }

            if (tokenEncrypted is not null)
            {
                tokenEncrypted = _encryptionService.Decrypt(tokenEncrypted);
            }

            return tokenEncrypted;
        }

        public async Task SetToken(byte[] token)
        {
            Task loadTask = _authenticationService.Load();

            byte[] tokenEncrypted = _encryptionService.Encrypt(token);

            await loadTask;

            _authenticationService.AuthenticationData.Twitch.Token = tokenEncrypted;

            await _authenticationService.Save();
        }

        public async Task DeleteToken()
        {
            await _authenticationService.Load();
            _authenticationService.AuthenticationData.Twitch.Token = null;
            await _authenticationService.Save();
        }

        public async Task<TwitchValidationResult> ValidateToken(byte[] token)
        {
            Token tokenData = new Token(_apiAuthConfiguration.TokenSchema, token);
            HttpRequest request = new HttpRequest(HttpMethodType.GET, _apiAuthConfiguration.Endpoints.Validate);
            request.Headers.Add("Authorization", $"{tokenData}");
            string response = await _httpClientService.SendRequestAsync(request);

            try
            {
                ValidateResponseSuccess success = JsonConvert.DeserializeObject<ValidateResponseSuccess>(response);
                return new TwitchValidationResult(success);
            }
            catch (JsonSerializationException)
            {}

            try
            {
                ValidateResponseFail fail = JsonConvert.DeserializeObject<ValidateResponseFail>(response);
                return new TwitchValidationResult(fail);
            }
            catch (JsonSerializationException)
            {}

            throw new Exception(response);
        }

        public bool AuthenticationPageClosePredicate(string url)
        {
            bool close = url.StartsWith(_authenticationConfiguration.RedirectUrl);
            return close;
        }

        #endregion
    }
}
