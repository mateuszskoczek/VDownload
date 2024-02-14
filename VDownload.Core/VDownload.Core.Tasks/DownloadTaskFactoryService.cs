using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Services.UI.Notifications;
using VDownload.Services.UI.StringResources;
using VDownload.Services.Utility.FFmpeg;

namespace VDownload.Core.Tasks
{
    public interface IDownloadTaskFactoryService
    {
        DownloadTask Create(Video video, VideoDownloadOptions downloadOptions);
    }



    public class DownloadTaskFactoryService : IDownloadTaskFactoryService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;
        protected readonly IFFmpegService _ffmpegService;
        protected readonly IStringResourcesService _stringResourcesService;
        protected readonly INotificationsService _notificationsService;

        #endregion



        #region CONSTRUCTORS

        public DownloadTaskFactoryService(IConfigurationService configurationService, ISettingsService settingsService, IFFmpegService ffmpegService, IStringResourcesService stringResourcesService, INotificationsService notificationsService)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;
            _ffmpegService = ffmpegService;
            _stringResourcesService = stringResourcesService;
            _notificationsService = notificationsService;
        }

        #endregion



        #region PUBLIC METHODS

        public DownloadTask Create(Video video, VideoDownloadOptions downloadOptions)
        {
            return new DownloadTask(video, downloadOptions, _configurationService, _settingsService, _ffmpegService, _stringResourcesService, _notificationsService);
        }

        #endregion
    }
}
