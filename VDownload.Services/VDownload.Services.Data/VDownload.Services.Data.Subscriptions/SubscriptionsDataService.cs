using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Common;
using VDownload.Services.Data.Configuration;

namespace VDownload.Services.Data.Subscriptions
{
    public interface ISubscriptionsDataService : IInitializableService
    {
        #region PROPERTIES

        ICollection<Subscription> Data { get; }

        #endregion



        #region METHODS

        Task Load();
        Task Save();

        #endregion
    }



    public class SubscriptionsDataService : ISubscriptionsDataService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;

        #endregion



        #region FIELDS

        protected readonly string _filePath;

        #endregion



        #region PROPERTIES

        public ICollection<Subscription> Data { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public SubscriptionsDataService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appdataDirectoryName = _configurationService.Common.Path.Appdata.DirectoryName;
            string appdataAuthenticationFilename = _configurationService.Common.Path.Appdata.SubscriptionsFile;
            _filePath = Path.Combine(appdataPath, appdataDirectoryName, appdataAuthenticationFilename);
        }

        #endregion



        #region PUBLIC METHODS

        public async Task Initialize() => await Load();

        public async Task Load()
        {
            if (File.Exists(_filePath))
            {
                string content = await File.ReadAllTextAsync(_filePath);
                Data = JsonConvert.DeserializeObject<ICollection<Subscription>>(content);
            }
            else
            {
                Data = new List<Subscription>();
            }
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
