// System
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VDownload.Services
{
    internal class Media
    {
        #region VARIABLES

        // PROGRESS UI ELEMENTS
        private TextBlock ProgressLabelTextblock;
        private ProgressBar ProgressBar;

        #endregion



        #region MAIN

        // TRANSCODE MEDIA FILE
        public async Task Transcode(StorageFile inputFile, StorageFile outputFile, string extension, string mediaType, TimeSpan duration, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken token, TextBlock progressLabelTextblock, ProgressBar progressBar)
        {
            // Set progress UI elements
            ProgressLabelTextblock = progressLabelTextblock;
            ProgressBar = progressBar;

            // Init transcoder
            MediaTranscoder transcoder = new MediaTranscoder
            {
                HardwareAccelerationEnabled = Config.GetValue("use_hardware_acceleration") == "1" ? true : false,
                VideoProcessingAlgorithm = Config.GetValue("use_mrfcrf444") == "1" ? MediaVideoProcessingAlgorithm.MrfCrf444 : MediaVideoProcessingAlgorithm.Default
            };
            if (0 < trimStart.TotalMilliseconds && trimStart.TotalMilliseconds < duration.TotalMilliseconds) // Set trimming at start
            {
                transcoder.TrimStartTime = trimStart;
            }
            if (0 < trimEnd.TotalMilliseconds && trimEnd.TotalMilliseconds < duration.TotalMilliseconds) // Set trimming at end
            {
                transcoder.TrimStopTime = trimEnd;
            }

            // Set video encoding profile
            MediaEncodingProfile profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
            switch (extension)
            {
                case "MP4": profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p); break;
                case "WMV": profile = MediaEncodingProfile.CreateWmv(VideoEncodingQuality.HD1080p); break;
                case "HEVC": profile = MediaEncodingProfile.CreateHevc(VideoEncodingQuality.HD1080p); break;
                case "MP3": profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High); break;
                case "FLAC": profile = MediaEncodingProfile.CreateFlac(AudioEncodingQuality.High); break;
                case "WAV": profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High); break;
                case "M4A": profile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.High); break;
                case "ALAC": profile = MediaEncodingProfile.CreateAlac(AudioEncodingQuality.High); break;
                case "WMA": profile = MediaEncodingProfile.CreateWma(AudioEncodingQuality.High); break;
            }
            var videoData = await inputFile.Properties.GetVideoPropertiesAsync();
            var audioData = await inputFile.Properties.GetMusicPropertiesAsync();
            if (mediaType != "A")
            {
                profile.Video.Height = videoData.Height;
                profile.Video.Width = videoData.Width;
                profile.Video.Bitrate = videoData.Bitrate - audioData.Bitrate;
            }
            if (mediaType != "V")
            {
                profile.Audio.Bitrate = audioData.Bitrate;
            }
            if (mediaType == "V")
            {
                var audioTracks = profile.GetAudioTracks();
                audioTracks.Clear();
                profile.SetAudioTracks(audioTracks.AsEnumerable());
            }

            // Start transcoding operation
            using (IRandomAccessStream outputFileOpened = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                PrepareTranscodeResult transcodingPreparated = await transcoder.PrepareStreamTranscodeAsync(await inputFile.OpenAsync(FileAccessMode.Read), outputFileOpened, profile);
                IAsyncActionWithProgress<double> transcodingTask = transcodingPreparated.TranscodeAsync();
                try
                {
                    await transcodingTask.AsTask(token, new Progress<double>(OnProgress));
                    await outputFileOpened.FlushAsync();
                }
                catch (TaskCanceledException) { }
                transcodingTask.Close();
            }
        }

        #endregion



        #region EVENTS

        // ON PROGRESS
        private void OnProgress(double percent)
        {
            // Set progress
            ProgressLabelTextblock.Text = $"{ResourceLoader.GetForCurrentView().GetString("VideoPanelProgressLabelTranscoding")} ({Math.Floor(percent)}%)";
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.Value = percent;
        }

        #endregion
    }
}
