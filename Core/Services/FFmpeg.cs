// System
using System.IO;
using System.Threading.Tasks;

// External
using FFMpegCore;
using FFMpegCore.Enums;



namespace VDownload.Core.Services
{
    class FFmpeg
    {
        #region CONSTANTS

        private static readonly FFOptions FFmpegOptions = new()
        {
            BinaryFolder = Config.Read("ffmpeg_executables_path"),
            TemporaryFilesFolder = Config.Read("temp_ffmpeg_path"),
        };

        #endregion



        #region INIT

        static FFmpeg()
        {
            // Create temporary directory
            Directory.CreateDirectory(Config.Read("temp_ffmpeg_path"));

            // Apply options
            GlobalFFOptions.Configure(FFmpegOptions);
        }

        #endregion



        #region MAIN

        public static async Task Mux(string videoPath, string audioPath, string outputPath)
        {
            await FFMpegArguments.FromFileInput(videoPath)
                                 .AddFileInput(audioPath)
                                 .OutputToFile(outputPath, true, options => options
                                    .UsingMultithreading(true)
                                    .WithSpeedPreset(Speed.UltraFast)
                                    .CopyChannel()
                                    .WithAudioCodec(AudioCodec.Aac)
                                    .WithAudioBitrate(AudioQuality.Good)
                                    .UsingShortest(false)
                                 )
                                 .ProcessAsynchronously();
        }

        public static async Task Convert(string inputPath, string outputPath)
        {
            await FFMpegArguments.FromFileInput(inputPath)
                                 .OutputToFile(outputPath, true, options => options
                                    .UsingMultithreading(true)
                                    .WithSpeedPreset(Speed.UltraFast)
                                 )
                                 .ProcessAsynchronously();
        }

        public static async Task Trim(string inputPath, string outputPath, double start, double end)
        {
            await FFMpegArguments.FromFileInput(inputPath)
                                 .OutputToFile(outputPath, true, options => options
                                    .UsingMultithreading(true)
                                    .WithSpeedPreset(Speed.UltraFast)
                                    .WithCustomArgument($"-ss {start.ToString().Replace(',', '.')}")
                                    .WithCustomArgument($"-t {end.ToString().Replace(',', '.')}")
                                 )
                                 .ProcessAsynchronously();
        }

        public static async Task OnlyVideo(string inputPath, string outputPath)
        {
            await FFMpegArguments.FromFileInput(inputPath)
                                 .OutputToFile(outputPath, true, options => options
                                     .UsingMultithreading(true)
                                     .WithSpeedPreset(Speed.UltraFast)
                                     .CopyChannel(Channel.Video)
                                     .DisableChannel(Channel.Audio)
                                 )
                                 .ProcessAsynchronously();
        }

        public static async Task OnlyAudio(string inputPath, string outputPath)
        {
            await FFMpegArguments.FromFileInput(inputPath)
                                 .OutputToFile(outputPath, true, options => options
                                     .UsingMultithreading(true)
                                     .WithSpeedPreset(Speed.UltraFast)
                                     .DisableChannel(Channel.Video)
                                 )
                                 .ProcessAsynchronously();
        }

        #endregion
    }
}
