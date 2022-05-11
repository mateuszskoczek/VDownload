using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Structs;
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
        #region PUBLIC METHODS

        public async Task Run(StorageFile mediaFile, MediaFileExtension extension, MediaType mediaType, StorageFile outputFile, TrimData trim, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

            MediaTranscoder mediaTranscoder = new MediaTranscoder
            {
                HardwareAccelerationEnabled = (bool)Config.GetValue("media_transcoding_use_hardware_acceleration"),
                VideoProcessingAlgorithm = (MediaVideoProcessingAlgorithm)Config.GetValue("media_transcoding_algorithm"),
            };
            if (trim.Start != null) mediaTranscoder.TrimStartTime = trim.Start;
            if (trim.End != null) mediaTranscoder.TrimStopTime = trim.End;

            using (IRandomAccessStream openedOutputFile = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                PrepareTranscodeResult transcodingPreparated = await mediaTranscoder.PrepareStreamTranscodeAsync(await mediaFile.OpenAsync(FileAccessMode.Read), openedOutputFile, await GetMediaEncodingProfile(mediaFile, extension, mediaType));
                
                IAsyncActionWithProgress<double> transcodingTask = transcodingPreparated.TranscodeAsync();
                await transcodingTask.AsTask(cancellationToken, new Progress<double>((percent) => { ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(percent)); }));
                cancellationToken.ThrowIfCancellationRequested();

                await openedOutputFile.FlushAsync();
                transcodingTask.Close();
            }

            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(100, true));
        }
        public async Task Run(StorageFile audioFile, StorageFile videoFile, VideoFileExtension extension, StorageFile outputFile, TrimData trim, CancellationToken cancellationToken = default) 
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(0));

            MediaComposition mediaEditor = new MediaComposition();

            cancellationToken.ThrowIfCancellationRequested();
            Task<MediaClip> getVideoFileTask = MediaClip.CreateFromFileAsync(videoFile).AsTask();
            Task<BackgroundAudioTrack> getAudioFileTask = BackgroundAudioTrack.CreateFromFileAsync(audioFile).AsTask();
            await Task.WhenAll(getVideoFileTask, getAudioFileTask);

            MediaClip videoElement = getVideoFileTask.Result;
            if (trim.Start != null) videoElement.TrimTimeFromStart = trim.Start;
            if (trim.End != null) videoElement.TrimTimeFromEnd = trim.End;
            BackgroundAudioTrack audioElement = getAudioFileTask.Result;
            if (trim.Start != null) audioElement.TrimTimeFromStart = trim.Start;
            if (trim.End != null) audioElement.TrimTimeFromEnd = trim.End;

            mediaEditor.Clips.Add(videoElement);
            mediaEditor.BackgroundAudioTracks.Add(audioElement);

            var renderOperation = mediaEditor.RenderToFileAsync(outputFile, (MediaTrimmingPreference)Config.GetValue("media_editing_algorithm"), await GetMediaEncodingProfile(videoFile, audioFile, (MediaFileExtension)extension, MediaType.AudioVideo));
            renderOperation.Progress += (info, progress) => { ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(progress)); };
            cancellationToken.ThrowIfCancellationRequested();
            await renderOperation.AsTask(cancellationToken);

            ProgressChanged(this, new EventArgs.ProgressChangedEventArgs(100, true));
        }
        public async Task Run(StorageFile audioFile, AudioFileExtension extension, StorageFile outputFile, TrimData trim, CancellationToken cancellationToken = default) 
        { 
            await Run(audioFile, (MediaFileExtension)extension, MediaType.OnlyAudio, outputFile, trim, cancellationToken); 
        }
        public async Task Run(StorageFile videoFile, VideoFileExtension extension, StorageFile outputFile, TrimData trim, CancellationToken cancellationToken = default)
        { 
            await Run(videoFile, (MediaFileExtension)extension, MediaType.OnlyVideo, outputFile, trim, cancellationToken); 
        }

        #endregion



        #region PRIVATE METHODS

        private static async Task<MediaEncodingProfile> GetMediaEncodingProfile(StorageFile videoFile, StorageFile audioFile, MediaFileExtension extension, MediaType mediaType)
        {
            MediaEncodingProfile profile;

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

            if (mediaType != MediaType.OnlyAudio)
            {
                var videoData = await videoFile.Properties.GetVideoPropertiesAsync();
                profile.Video.Height = videoData.Height;
                profile.Video.Width = videoData.Width;
                profile.Video.Bitrate = videoData.Bitrate;
            }
            if (mediaType != MediaType.OnlyVideo)
            {
                var audioData = await audioFile.Properties.GetMusicPropertiesAsync();
                profile.Audio.Bitrate = audioData.Bitrate;
                if (mediaType == MediaType.AudioVideo) profile.Video.Bitrate -= audioData.Bitrate;
            }
            if (mediaType == MediaType.OnlyVideo)
            {
                var audioTracks = profile.GetAudioTracks();
                audioTracks.Clear();
                profile.SetAudioTracks(audioTracks.AsEnumerable());
            }

            return profile;
        }
        private static async Task<MediaEncodingProfile> GetMediaEncodingProfile(StorageFile audioVideoFile, MediaFileExtension extension, MediaType mediaType)
        { 
            return await GetMediaEncodingProfile(audioVideoFile, audioVideoFile, extension, mediaType);  
        }

        #endregion



        #region EVENTS

        public event EventHandler<EventArgs.ProgressChangedEventArgs> ProgressChanged;

        #endregion
    }
}
