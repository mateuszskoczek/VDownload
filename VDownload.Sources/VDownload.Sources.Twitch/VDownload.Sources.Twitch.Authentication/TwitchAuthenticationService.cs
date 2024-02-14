using Newtonsoft.Json;
using VDownload.Services.Data.Authentication;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Utility.Encryption;
using VDownload.Sources.Twitch.Api;
using VDownload.Sources.Twitch.Authentication.Models;

namespace VDownload.Sources.Twitch.Authentication
{
    public interface ITwitchAuthenticationService
    {
        Task DeleteToken();
        Task<byte[]?> GetToken();
        Task SetToken(byte[] token);
        Task<TwitchValidationResult> ValidateToken(byte[] token);
    }



    public class TwitchAuthenticationService : ITwitchAuthenticationService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ITwitchApiService _apiService;
        protected readonly IAuthenticationDataService _authenticationDataService;
        protected readonly IEncryptionService _encryptionService;

        #endregion



        #region CONSTRUCTORS

        public TwitchAuthenticationService(IConfigurationService configurationService, ITwitchApiService apiService, IAuthenticationDataService authenticationDataService, IEncryptionService encryptionService)
        {
            _configurationService = configurationService;
            _apiService = apiService;
            _authenticationDataService = authenticationDataService;
            _encryptionService = encryptionService;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<byte[]?> GetToken()
        {
            await _authenticationDataService.Load();

            byte[]? tokenEncrypted = _authenticationDataService.Data.Twitch.Token;

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
            Task loadTask = _authenticationDataService.Load();

            byte[] tokenEncrypted = _encryptionService.Encrypt(token);

            await loadTask;

            _authenticationDataService.Data.Twitch.Token = tokenEncrypted;

            await _authenticationDataService.Save();
        }

        public async Task DeleteToken()
        {
            await _authenticationDataService.Load();
            _authenticationDataService.Data.Twitch.Token = null;
            await _authenticationDataService.Save();
        }

        public async Task<TwitchValidationResult> ValidateToken(byte[] token)
        {
            string response = await _apiService.AuthValidate(token);

            try
            {
                ValidateResponseSuccess success = JsonConvert.DeserializeObject<ValidateResponseSuccess>(response);
                return new TwitchValidationResult(success);
            }
            catch (JsonSerializationException)
            { }

            try
            {
                ValidateResponseFail fail = JsonConvert.DeserializeObject<ValidateResponseFail>(response);
                return new TwitchValidationResult(fail);
            }
            catch (JsonSerializationException)
            { }

            throw new Exception(response);
        }

        #endregion
    }
}
