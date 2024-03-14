﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Common;
using VDownload.Services.Data.Configuration;

namespace VDownload.Services.Data.Settings
{
    public interface ISettingsService : IInitializableService
    {
        #region PROPERTIES

        SettingsData Data { get; }

        #endregion



        #region METHODS

        Task Load();
        Task Save();
        Task Restore();

        #endregion
    }



    public class SettingsService : ISettingsService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;

        #endregion



        #region FIELDS

        protected readonly string _filePath;

        #endregion



        #region PROPERTIES

        public SettingsData Data { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public SettingsService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appdataDirectoryName = _configurationService.Common.Path.Appdata.DirectoryName;
            string appdataAuthenticationFilename = _configurationService.Common.Path.Appdata.DataFile;
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
                Data = JsonConvert.DeserializeObject<SettingsData>(content);
            }
            else
            {
                Data = new SettingsData();
            }
        }

        public async Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            string content = JsonConvert.SerializeObject(Data);
            await File.WriteAllTextAsync(_filePath, content);
        }

        public async Task Restore()
        {
            Data = new SettingsData();
            await Save();
        }

        #endregion
    }
}
