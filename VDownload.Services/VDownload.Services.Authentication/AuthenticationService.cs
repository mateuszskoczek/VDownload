using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Extensions;

namespace VDownload.Services.Authentication
{
    public interface IAuthenticationService
    {
        #region PROPERTIES

        AuthenticationData? AuthenticationData { get; }

        #endregion



        #region METHODS

        Task Load();
        Task Save();

        #endregion
    }



    public class AuthenticationService : IAuthenticationService
    {
        #region SERVICES

        private AuthenticationConfiguration _configuration;

        #endregion



        #region FIELDS

        private string _authenticationDataFile;

        #endregion



        #region PROPERTIES

        public AuthenticationData AuthenticationData { get; private set; }

        public bool AuthenticationDataLoaded => AuthenticationData is not null;

        #endregion



        #region CONSTRUCTORS

        public AuthenticationService(AuthenticationConfiguration configuration)
        {
            _configuration = configuration;

            _authenticationDataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _configuration.FilePath);
        }

        #endregion



        #region PUBLIC METHODS

        public async Task Load()
        {
            AuthenticationData = null;

            if (File.Exists(_authenticationDataFile))
            {
                string content = await File.ReadAllTextAsync(_authenticationDataFile);
                AuthenticationData = JsonConvert.DeserializeObject<AuthenticationData>(content);
            }

            AuthenticationData ??= new AuthenticationData();
        }

        public async Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_authenticationDataFile));
            string content = JsonConvert.SerializeObject(AuthenticationData);
            await File.WriteAllTextAsync(_authenticationDataFile, content);
        }

        #endregion
    }
}
