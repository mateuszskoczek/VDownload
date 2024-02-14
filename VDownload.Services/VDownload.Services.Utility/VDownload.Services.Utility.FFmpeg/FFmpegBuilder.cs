using FFMpegCore;
using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Services.Data.Settings;

namespace VDownload.Services.Utility.FFmpeg
{
    public class FFmpegBuilder
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ISettingsService _settingsService;

        #endregion



        #region FIELDS

        protected TimeSpan? _trimStart;
        protected TimeSpan? _trimEnd;

        protected bool _progressReporterJoined = false;
        protected Action<double> _progressReporter;
        protected TimeSpan _progressReporterVideoDuration;

        protected bool _cancellationTokenJoined = false;
        protected CancellationToken _cancellationToken;

        protected MediaType _mediaType = MediaType.Original;

        protected string _inputFile;
        protected string _outputFile;

        protected FFOptions _options;

        #endregion



        #region CONSTRUCTORS

        internal FFmpegBuilder(IConfigurationService configurationService, ISettingsService settingsService)
        {
            _configurationService = configurationService;
            _settingsService = settingsService;

            _options = new FFOptions
            {
                BinaryFolder = _settingsService.Data.Common.Processing.FFmpegLocation,
            };
        }

        #endregion



        #region PUBLIC METHODS

        public FFmpegBuilder SetMediaType(MediaType mediaType)
        {
            _mediaType = mediaType;
            return this;
        }

        public FFmpegBuilder TrimStart(TimeSpan start)
        {
            _trimStart = start;
            return this;
        }

        public FFmpegBuilder TrimEnd(TimeSpan end)
        {
            _trimEnd = end;
            return this;
        }

        public FFmpegBuilder JoinProgressReporter(Action<double> progressReporter, TimeSpan videoDuration)
        {
            _progressReporterJoined = true;
            _progressReporter = progressReporter;
            _progressReporterVideoDuration = videoDuration;
            return this;
        }

        public FFmpegBuilder JoinCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationTokenJoined = true;
            _cancellationToken = cancellationToken;
            return this;
        }

        public async Task RunAsync(string inputFile, string outputFile)
        {
            _inputFile = inputFile;
            _outputFile = outputFile;

            FFMpegArgumentProcessor ffmpegArguments = FFMpegArguments.FromFileInput(inputFile, true, async (options) => await BuildInputArgumentOptions(options))
                                                                     .OutputToFile(outputFile, true, async (options) => await BuildOutputArgumentOptions(options));

            if (_cancellationTokenJoined)
            {
                ffmpegArguments = ffmpegArguments.CancellableThrough(_cancellationToken);
            }

            if (_progressReporterJoined)
            {
                ffmpegArguments = ffmpegArguments.NotifyOnProgress(_progressReporter, _progressReporterVideoDuration);
            }

            try
            {
                await ffmpegArguments.ProcessAsynchronously(true, _options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #endregion



        #region PRIVATE METHODS

        private async Task BuildInputArgumentOptions(FFMpegArgumentOptions options)
        {
            options.UsingMultithreading(_settingsService.Data.Common.Processing.UseMultithreading);
            options.WithSpeedPreset((Speed)_settingsService.Data.Common.Processing.Speed);
            if (_settingsService.Data.Common.Processing.UseHardwareAcceleration)
            {
                options.WithHardwareAcceleration(HardwareAccelerationDevice.Auto);
            }

            if (_trimStart is not null)
            {
                options.WithCustomArgument($"-ss {_trimStart}");
            }

            if (_trimEnd is not null)
            {
                options.WithCustomArgument($"-to {_trimEnd}");
            }
        }

        private async Task BuildOutputArgumentOptions(FFMpegArgumentOptions options)
        {
            IMediaAnalysis analysis = await FFProbe.AnalyseAsync(_inputFile, _options);
            string audioCodec = analysis.AudioStreams.First().CodecName;
            string videoCodec = analysis.VideoStreams.First().CodecName;

            string extension = Path.GetExtension(_outputFile).Replace(".", string.Empty);
            Data.Configuration.Models.Muxer muxer = _configurationService.Common.Processing.Muxers.First(x => x.Extension == extension);

            if (_mediaType != MediaType.OnlyAudio)
            {
                IEnumerable<string> availableCodecs = muxer.VideoCodecs;
                string selectedCodec = availableCodecs.Contains(videoCodec) ? "copy" : availableCodecs.First();
                options.WithCustomArgument($"-vcodec {selectedCodec}");
            }
            else
            {
                options.WithCustomArgument("-vn");
            }

            if (_mediaType != MediaType.OnlyVideo)
            {
                IEnumerable<string> availableCodecs = muxer.AudioCodecs;
                string selectedCodec = availableCodecs.Contains(audioCodec) ? "copy" : availableCodecs.First();
                options.WithCustomArgument($"-acodec {selectedCodec}");
            }
            else
            {
                options.WithCustomArgument("-an");
            }
        }

        #endregion

    }
}
