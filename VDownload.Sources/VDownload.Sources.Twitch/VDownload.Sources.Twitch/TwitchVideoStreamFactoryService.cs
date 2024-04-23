using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Sources.Twitch.Models;

namespace VDownload.Sources.Twitch
{
    public interface ITwitchVideoStreamFactoryService
    {
        TwitchVodStream CreateVodStream(); 
        TwitchClipStream CreateClipStream();
    }



    public class TwitchVideoStreamFactoryService : ITwitchVideoStreamFactoryService
    {
        #region SERVICES

        protected readonly HttpClient _httpClient;

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region CONSTRUCTORS

        public TwitchVideoStreamFactoryService(HttpClient httpClient, IConfigurationService configurationService, ISettingsService settingsService)
        {
            _httpClient = httpClient;
            _configurationService = configurationService;
            _settingsService = settingsService;
        }

        #endregion



        #region PUBLIC METHODS

        public TwitchVodStream CreateVodStream() => new TwitchVodStream(_httpClient, _configurationService, _settingsService);

        public TwitchClipStream CreateClipStream() => new TwitchClipStream(_httpClient, _configurationService, _settingsService);

        #endregion
    }
}
