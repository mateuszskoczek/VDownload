using System;
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
        #region STANDARD METHODS

        // SINGLE AUDIO & VIDEO FILE PROCESSING
        public async Task Run(StorageFile mediaFile, MediaFileExtension extension, MediaType mediaType, StorageFile outputFile, TimeSpan? trimStart = null, TimeSpan? trimEnd = null, CancellationToken cancellationToken = default)
        {
            // Invoke event at start
            cancellationToken.ThrowIfCancellationRequested();
            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

            // Init transcoder
            MediaTranscoder mediaTranscoder = new MediaTranscoder
            {
                HardwareAccelerationEnabled = (bool)Config.GetValue("media_transcoding_use_hardware_acceleration"),
                VideoProcessingAlgorithm = (MediaVideoProcessingAlgorithm)Config.GetValue("media_transcoding_algorithm"),
            };
            if (trimStart != null) mediaTranscoder.TrimStartTime = (TimeSpan)trimStart;
            if (trimEnd != null) mediaTranscoder.TrimStopTime = (TimeSpan)trimEnd;

            // Start transcoding operation
            using (IRandomAccessStream openedOutputFile = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Prepare transcode task
                PrepareTranscodeResult transcodingPreparated = await mediaTranscoder.PrepareStreamTranscodeAsync(await mediaFile.OpenAsync(FileAccessMode.Read), openedOutputFile, await GetMediaEncodingProfile(mediaFile, extension, mediaType));
                
                // Start transcoding
                IAsyncActionWithProgress<double> transcodingTask = transcodingPreparated.TranscodeAsync();
                await transcodingTask.AsTask(cancellationToken, new Progress<double>((percent) => { ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(percent)); }));
                cancellationToken.ThrowIfCancellationRequested();

                // Finalizing
                await openedOutputFile.FlushAsync();
                transcodingTask.Close();
            }

            // Invoke event at end
            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(100, true));
        }

        // SEPARATE AUDIO & VIDEO FILES PROCESSING
        public async Task Run(StorageFile audioFile, StorageFile videoFile, VideoFileExtension extension, StorageFile outputFile, TimeSpan? trimStart = null, TimeSpan? trimEnd = null, CancellationToken cancellationToken = default) 
        {
            // Invoke event at start
            cancellationToken.ThrowIfCancellationRequested();
            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

            // Init editor
            MediaComposition mediaEditor = new MediaComposition();

            // Add media files
            cancellationToken.ThrowIfCancellationRequested();
            Task<MediaClip> getVideoFileTask = MediaClip.CreateFromFileAsync(videoFile).AsTask();
            Task<BackgroundAudioTrack> getAudioFileTask = BackgroundAudioTrack.CreateFromFileAsync(audioFile).AsTask();
            await Task.WhenAll(getVideoFileTask, getAudioFileTask);

            MediaClip videoElement = getVideoFileTask.Result;
            if (trimStart != null) videoElement.TrimTimeFromStart = (TimeSpan)trimStart;
            if (trimEnd != null) videoElement.TrimTimeFromEnd = (TimeSpan)trimEnd;
            BackgroundAudioTrack audioElement = getAudioFileTask.Result;
            if (trimStart != null) audioElement.TrimTimeFromStart = (TimeSpan)trimStart;
            if (trimEnd != null) audioElement.TrimTimeFromEnd = (TimeSpan)trimEnd;

            mediaEditor.Clips.Add(videoElement);
            mediaEditor.BackgroundAudioTracks.Add(audioElement);

            // Start rendering operation
            var renderOperation = mediaEditor.RenderToFileAsync(outputFile, (MediaTrimmingPreference)Config.GetValue("media_editing_algorithm"), await GetMediaEncodingProfile(videoFile, audioFile, (MediaFileExtension)extension, MediaType.AudioVideo));
            renderOperation.Progress += (info, progress) => { ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(progress)); };
            cancellationToken.ThrowIfCancellationRequested();
            await renderOperation.AsTask(cancellationToken);

            // Invoke event at end
            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(100, true));
        }

        // AUDIO FILE PROCESSING
        public async Task Run(StorageFile audioFile, AudioFileExtension extension, StorageFile outputFile, TimeSpan? trimStart = null, TimeSpan? trimEnd = null, CancellationToken cancellationToken = default) 
        { 
            await Run(audioFile, (MediaFileExtension)extension, MediaType.OnlyAudio, outputFile, trimStart, trimEnd, cancellationToken); 
        }

        // VIDEO FILE PROCESSING
        public async Task Run(StorageFile videoFile, VideoFileExtension extension, StorageFile outputFile, TimeSpan? trimStart = null, TimeSpan? trimEnd = null, CancellationToken cancellationToken = default)
        { 
            await Run(videoFile, (MediaFileExtension)extension, MediaType.OnlyVideo, outputFile, trimStart, trimEnd, cancellationToken); 
        }

        #endregion



        #region LOCAL METHODS

        // GET ENCODING PROFILE
        private static async Task<MediaEncodingProfile> GetMediaEncodingProfile(StorageFile videoFile, StorageFile audioFile, MediaFileExtension extension, MediaType mediaType)
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
        private static async Task<MediaEncodingProfile> GetMediaEncodingProfile(StorageFile audioVideoFile, MediaFileExtension extension, MediaType mediaType)
        { 
            return await GetMediaEncodingProfile(audioVideoFile, audioVideoFile, extension, mediaType);  
        }

        #endregion



        #region EVENT HANDLERS

        public event EventHandler<EventArgs.ProgressChangedEventArgs> ProgressChanged;

        #endregion
    }
}
