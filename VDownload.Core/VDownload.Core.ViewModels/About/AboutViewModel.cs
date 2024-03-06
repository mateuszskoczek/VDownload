using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.ViewModels.About.Helpers;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Configuration.Models;
using VDownload.Services.UI.StringResources;
using Windows.System.UserProfile;

namespace VDownload.Core.ViewModels.About
{
    public partial class AboutViewModel : ObservableObject
    {
        #region SERVICES

        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly IConfigurationService _configurationService;

        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected string _version;

        [ObservableProperty]
        protected ObservableCollection<PersonViewModel> _developers;

        [ObservableProperty]
        protected ObservableCollection<PersonViewModel> _translators;

        [ObservableProperty]
        protected Uri _repositoryUrl;

        [ObservableProperty]
        protected Uri _donationUrl;

        #endregion



        #region CONSTRUCTORS

        public AboutViewModel(IStringResourcesService stringResourcesService, IConfigurationService configurationService)
        {
            _stringResourcesService = stringResourcesService;
            _configurationService = configurationService;

            string version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (version == "0.0.0")
            {
                version = _stringResourcesService.AboutViewResources.Get("SelfbuiltVersion");
            }
            _version = version;

            _developers = new ObservableCollection<PersonViewModel>(_configurationService.Common.About.Developers.Select(x => new PersonViewModel(x.Name, x.Url)));

            _repositoryUrl = new Uri(_configurationService.Common.About.RepositoryUrl);
            _donationUrl = new Uri(_configurationService.Common.About.DonationUrl);
        }

        #endregion



        #region COMMANDS

        [RelayCommand]
        public void Navigation()
        {
            string languageCode = "en-US";
            Language language = _configurationService.Common.About.Translation.FirstOrDefault(x => x.Code == languageCode);
            Translators = new ObservableCollection<PersonViewModel>(language.Translators.Select(x => new PersonViewModel(x.Name, x.Url)));
        }

        #endregion
    }
}
