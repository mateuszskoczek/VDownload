using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using Windows.Foundation;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VDownload.Core.Services
{
    public class MediaProcessor
    {
        #region CONSTRUCTORS

        public MediaProcessor(StorageFile outputFile, TimeSpan trimStart, TimeSpan trimEnd)
        {
            OutputFile = outputFile;
            TrimStart = trimStart;
            TrimEnd = trimEnd;
        }

        #endregion



        #region PROPERTIES

        public StorageFile OutputFile { get; private set; }
        public TimeSpan TrimStart { get; private set; }
        public TimeSpan TrimEnd { get; private set; }

        #endregion



        #region STANDARD METHODS

        // SINGLE AUDIO & VIDEO FILE PROCESSING
        public async Task Run(StorageFile audioVideoInputFile, MediaFileExtension extension, MediaType mediaType, CancellationToken cancellationToken = default)
        {
            // Invoke ProcessingStarted event
            ProcessingStarted?.Invoke(this, System.EventArgs.Empty);

            // Init transcoder
            MediaTranscoder mediaTranscoder = new MediaTranscoder
            {
                HardwareAccelerationEnabled = (bool)Config.GetValue("media_transcoding_use_hardware_acceleration"),
                VideoProcessingAlgorithm = (bool)Config.GetValue("media_transcoding_use_mrfcrf444_algorithm") ? MediaVideoProcessingAlgorithm.MrfCrf444 : MediaVideoProcessingAlgorithm.Default,
                TrimStartTime = TrimStart,
                TrimStopTime = TrimEnd,
            };

            // Start transcoding operation
            cancellationToken.ThrowIfCancellationRequested();
            using (IRandomAccessStream outputFileOpened = await OutputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                PrepareTranscodeResult transcodingPreparated = await mediaTranscoder.PrepareStreamTranscodeAsync(await audioVideoInputFile.OpenAsync(FileAccessMode.Read), outputFileOpened, await GetMediaEncodingProfile(audioVideoInputFile, extension, mediaType));
                IAsyncActionWithProgress<double> transcodingTask = transcodingPreparated.TranscodeAsync();
                await transcodingTask.AsTask(cancellationToken, new Progress<double>((percent) => { ProcessingProgressChanged(this, new ProgressChangedEventArgs((int)Math.Round(percent), null)); }));
                await outputFileOpened.FlushAsync();
                transcodingTask.Close();
            }

            // Invoke ProcessingCompleted event
            ProcessingCompleted?.Invoke(this, System.EventArgs.Empty);
        }

        // SEPARATE AUDIO & VIDEO FILES PROCESSING
        public async Task Run(StorageFile audioFile, StorageFile videoFile, VideoFileExtension extension, CancellationToken cancellationToken = default) 
        {
            // Invoke ProcessingStarted event
            ProcessingStarted?.Invoke(this, System.EventArgs.Empty);

            // Init editor
            MediaComposition mediaEditor = new MediaComposition();

            // Add media files
            cancellationToken.ThrowIfCancellationRequested();
            Task<MediaClip> getVideoFileTask = MediaClip.CreateFromFileAsync(videoFile).AsTask();
            Task<BackgroundAudioTrack> getAudioFileTask = BackgroundAudioTrack.CreateFromFileAsync(audioFile).AsTask();
            await Task.WhenAll(getVideoFileTask, getAudioFileTask);

            MediaClip videoElement = getVideoFileTask.Result;
            videoElement.TrimTimeFromStart = TrimStart;
            videoElement.TrimTimeFromEnd = TrimEnd;
            BackgroundAudioTrack audioElement = getAudioFileTask.Result;
            audioElement.TrimTimeFromStart = TrimStart;
            audioElement.TrimTimeFromEnd = TrimEnd;

            mediaEditor.Clips.Add(getVideoFileTask.Result);
            mediaEditor.BackgroundAudioTracks.Add(getAudioFileTask.Result);

            // Start rendering operation
            var renderOperation = mediaEditor.RenderToFileAsync(OutputFile, (MediaTrimmingPreference)Config.GetValue("media_editing_algorithm"), await GetMediaEncodingProfile(videoFile, audioFile, (MediaFileExtension)extension, MediaType.AudioVideo));
            renderOperation.Progress += (info, progress) => { ProcessingProgressChanged(this, new ProgressChangedEventArgs((int)Math.Round(progress), null)); };
            cancellationToken.ThrowIfCancellationRequested();
            await renderOperation.AsTask(cancellationToken);

            // Invoke ProcessingCompleted event
            ProcessingCompleted?.Invoke(this, System.EventArgs.Empty);
        }

        // SINGLE AUDIO OR VIDEO FILES PROCESSING
        public async Task Run(StorageFile audioFile, AudioFileExtension extension, CancellationToken cancellationToken = default) { await Run(audioFile, (MediaFileExtension)extension, MediaType.OnlyAudio, cancellationToken); }
        public async Task Run(StorageFile videoFile, VideoFileExtension extension, CancellationToken cancellationToken = default) { await Run(videoFile, (MediaFileExtension)extension, MediaType.OnlyVideo, cancellationToken); }

        #endregion



        #region LOCAL METHODS

        // GET ENCODING PROFILE
        public static async Task<MediaEncodingProfile> GetMediaEncodingProfile(StorageFile videoFile, StorageFile audioFile, MediaFileExtension extension, MediaType mediaType)
        {
            // Create profile object
            MediaEncodingProfile profile;

            // Set extension
            switch (extension)
            {
                default:
                case MediaFileExtension.MP4: profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p); break;
                case MediaFileExtension.WMV: profile = MediaEncodingProfile.CreateWmv(VideoEncodingQuality.HD1080p); break;
                case MediaFileExtension.HEVC: profile = MediaEncodingProfile.CreateHevc(VideoEncodingQuality.HD1080p); break;
                case MediaFileExtension.MP3: profile = MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High); break;
                case MediaFileExtension.FLAC: profile = MediaEncodingProfile.CreateFlac(AudioEncodingQuality.High); break;
                case MediaFileExtension.WAV: profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High); break;
                case MediaFileExtension.M4A: profile = MediaEncodingProfile.CreateM4a(AudioEncodingQuality.High); break;
                case MediaFileExtension.ALAC: profile = MediaEncodingProfile.CreateAlac(AudioEncodingQuality.High); break;
                case MediaFileExtension.WMA: profile = MediaEncodingProfile.CreateWma(AudioEncodingQuality.High); break;
            }

            // Set video parameters
            if (mediaType != MediaType.OnlyAudio)
            {
                var videoData = await videoFile.Properties.GetVideoPropertiesAsync();
                profile.Video.Height = videoData.Height;
                profile.Video.Width = videoData.Width;
                profile.Video.Bitrate = videoData.Bitrate;
            }

            // Set audio parameters
            if (mediaType != MediaType.OnlyVideo)
            {
                var audioData = await audioFile.Properties.GetMusicPropertiesAsync();
                profile.Audio.Bitrate = audioData.Bitrate;
                if (mediaType == MediaType.AudioVideo) profile.Video.Bitrate -= audioData.Bitrate;
            }

            // Delete audio tracks
            if (mediaType == MediaType.OnlyVideo)
            {
                var audioTracks = profile.GetAudioTracks();
                audioTracks.Clear();
                profile.SetAudioTracks(audioTracks.AsEnumerable());
            }

            // Return profile
            return profile;
        }
        public static async Task<MediaEncodingProfile> GetMediaEncodingProfile(StorageFile audioVideoFile, MediaFileExtension extension, MediaType mediaType) { return await GetMediaEncodingProfile(audioVideoFile, audioVideoFile, extension, mediaType);  }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler ProcessingStarted;
        public event EventHandler<ProgressChangedEventArgs> ProcessingProgressChanged;
        public event EventHandler ProcessingCompleted;

        #endregion
    }
}
