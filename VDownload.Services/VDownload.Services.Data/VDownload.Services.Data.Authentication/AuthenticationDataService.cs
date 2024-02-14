using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Data.Configuration;

namespace VDownload.Services.Data.Authentication
{
    public interface IAuthenticationDataService
    {
        #region PROPERTIES

        AuthenticationData Data { get; }

        #endregion



        #region METHODS

        Task Load();
        Task Save();

        #endregion
    }

    public class AuthenticationDataService : IAuthenticationDataService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;

        #endregion



        #region FIELDS

        protected readonly string _filePath;

        #endregion



        #region PROPERTIES

        public AuthenticationData Data { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public AuthenticationDataService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appdataDirectoryName = _configurationService.Common.Path.Appdata.DirectoryName;
            string appdataAuthenticationFilename = _configurationService.Common.Path.Appdata.AuthenticationFile;
            _filePath = Path.Combine(appdataPath, appdataDirectoryName, appdataAuthenticationFilename);
        }

        #endregion



        #region PUBLIC METHODS

        public async Task Load()
        {
            Data = null;

            if (File.Exists(_filePath))
            {
                string content = await File.ReadAllTextAsync(_filePath);
                Data = JsonConvert.DeserializeObject<AuthenticationData>(content);
            }

            Data ??= new AuthenticationData();
        }

        public async Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            string content = JsonConvert.SerializeObject(Data);
            await File.WriteAllTextAsync(_filePath, content);
        }

        #endregion
    }
}
