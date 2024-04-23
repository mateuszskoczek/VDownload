using FFMpegCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;

namespace VDownload.Services.Utility.FFmpeg
{
    public interface IFFmpegService
    {
        FFmpegBuilder CreateBuilder();
    }



    public class FFmpegService : IFFmpegService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region CONSTRUCTORS

        public FFmpegService(IConfigurationService configurationService, ISettingsService settingsService)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;
        }

        #endregion



        #region PUBLIC METHODS

        public FFmpegBuilder CreateBuilder() => new FFmpegBuilder(_configurationService, _settingsService);

        #endregion
    }
}
