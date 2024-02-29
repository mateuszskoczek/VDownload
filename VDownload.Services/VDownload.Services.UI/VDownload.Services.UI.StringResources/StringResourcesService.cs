using VDownload.Services.Data.Configuration;
using Windows.ApplicationModel.Resources;

namespace VDownload.Services.UI.StringResources
{
    public interface IStringResourcesService
    {
        StringResources BaseViewResources { get; }
        StringResources HomeViewResources { get; }
        StringResources HomeVideoViewResources { get; }
        StringResources HomePlaylistViewResources { get; }
        StringResources HomeDownloadsViewResources { get; }
        StringResources AuthenticationViewResources { get; }
        StringResources NotificationsResources { get; }
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
        public StringResources HomePlaylistViewResources { get; protected set; }
        public StringResources HomeDownloadsViewResources { get; protected set; }
        public StringResources AuthenticationViewResources { get; protected set; }
        public StringResources NotificationsResources { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public StringResourcesService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;

            BaseViewResources = BuildResource("BaseViewResources");
            HomeViewResources = BuildResource("HomeViewResources");
            HomeVideoViewResources = BuildResource("HomeVideoViewResources");
            HomePlaylistViewResources = BuildResource("HomePlaylistViewResources");
            HomeDownloadsViewResources = BuildResource("HomeDownloadsViewResources");
            AuthenticationViewResources = BuildResource("AuthenticationViewResources");
            NotificationsResources = BuildResource("NotificationsResources");
        }

        #endregion



        #region PRIVATE METHODS

        protected StringResources BuildResource(string resourceName) => new StringResources(ResourceLoader.GetForViewIndependentUse($"{_configurationService.Common.StringResourcesAssembly}/{resourceName}"));

        #endregion
    }
}
