using SimpleToolkit.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Web;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;
using VDownload.Sources.Twitch.Models.Internal;

namespace VDownload.Sources.Twitch.Models
{
    public class TwitchClipStream : VideoStream
    {
        #region SERVICES

        protected readonly HttpClient _httpClient;

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region PROPERTIES

        public int Height { get; set; }
        public double FrameRate { get; set; }
        public Uri Url { get; set; }
        public string Signature { get; set; }
        public string Token { get; set; }

        #endregion



        #region CONSTRUCTORS

        public TwitchClipStream(HttpClient httpClient, IConfigurationService configurationService, ISettingsService settingsService)
        {
            _httpClient = httpClient;

            _configurationService = configurationService;
            _settingsService = settingsService;
        }

        #endregion



        #region PUBLIC METHODS

        public async override Task<VideoStreamDownloadResult> Download(string taskTemporaryDirectory, IProgress<double> onProgress, CancellationToken token, TimeSpan duration, TimeSpan trimStart, TimeSpan trimEnd)
        {
            token.ThrowIfCancellationRequested();

            string location = Path.Combine(taskTemporaryDirectory, _configurationService.Twitch.Download.Clip.FileName);

            string url = $"{Url.OriginalString}?sig={Signature}&token={HttpUtility.UrlEncode(Token)}";
            using (FileStream fileStream = File.Create(location))
            {
                await _httpClient.DownloadAsync(url, fileStream, token, onProgress);
                token.ThrowIfCancellationRequested();
            }

            return new VideoStreamDownloadResult 
            { 
                File = location, 
                NewDuration = duration, 
                NewTrimEnd = trimEnd, 
                NewTrimStart = trimStart
            };
        }

        #endregion
    }
}
