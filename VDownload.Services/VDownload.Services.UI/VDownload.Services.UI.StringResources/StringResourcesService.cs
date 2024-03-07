using VDownload.Services.Data.Configuration;
using Windows.ApplicationModel.Resources;

namespace VDownload.Services.UI.StringResources
{
    public interface IStringResourcesService
    {
        StringResources BaseViewResources { get; }
        StringResources HomeViewResources { get; }
        StringResources HomeVideoViewResources { get; }
        StringResources HomeVideoCollectionViewResources { get; }
        StringResources HomeDownloadsViewResources { get; }
        StringResources AuthenticationViewResources { get; }
        StringResources NotificationsResources { get; }
        StringResources SearchResources { get; }
        StringResources CommonResources { get; }
        StringResources DialogButtonsResources { get; }
        StringResources SettingsViewResources { get; }
        StringResources FilenameTemplateResources { get; }
        StringResources AboutViewResources { get; }
    }



    public class StringResourcesService : IStringResourcesService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;

        #endregion



        #region PROPERTIES

        public StringResources BaseViewResources { get; protected set; }
        public StringResources HomeViewResources { get; protected set; }
        public StringResources HomeVideoViewResources { get; protected set; }
        public StringResources HomeVideoCollectionViewResources { get; protected set; }
        public StringResources HomeDownloadsViewResources { get; protected set; }
        public StringResources AuthenticationViewResources { get; protected set; }
        public StringResources NotificationsResources { get; protected set; }
        public StringResources SearchResources { get; protected set; }
        public StringResources CommonResources { get; protected set; }
        public StringResources DialogButtonsResources { get; protected set; }
        public StringResources SettingsViewResources { get; protected set; }
        public StringResources FilenameTemplateResources { get; protected set; }
        public StringResources AboutViewResources { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public StringResourcesService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            BaseViewResources = BuildResource("BaseViewResources");
            HomeViewResources = BuildResource("HomeViewResources");
            HomeVideoViewResources = BuildResource("HomeVideoViewResources");
            HomeVideoCollectionViewResources = BuildResource("HomeVideoCollectionViewResources");
            HomeDownloadsViewResources = BuildResource("HomeDownloadsViewResources");
            AuthenticationViewResources = BuildResource("AuthenticationViewResources");
            NotificationsResources = BuildResource("NotificationsResources");
            SearchResources = BuildResource("SearchResources");
            CommonResources = BuildResource("CommonResources");
            DialogButtonsResources = BuildResource("DialogButtonsResources");
            SettingsViewResources = BuildResource("SettingsViewResources");
            FilenameTemplateResources = BuildResource("FilenameTemplateResources");
            AboutViewResources = BuildResource("AboutViewResources");
        }

        #endregion



        #region PRIVATE METHODS

        protected StringResources BuildResource(string resourceName) => new StringResources(ResourceLoader.GetForViewIndependentUse($"{_configurationService.Common.StringResourcesAssembly}/{resourceName}"));

        #endregion
    }
}
