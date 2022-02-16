using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Models;
using Windows.Storage;

namespace VDownload.Core.Interfaces
{
    public interface IVideoService
    {
        #region PARAMETERS

        string ID { get; }
        string Title { get; }
        string Author { get; }
        DateTime Date { get; }
        TimeSpan Duration { get; }
        long Views { get; }
        Uri Thumbnail { get; }

        #endregion



        #region METHODS

        // GET VIDEO METADATA
        Task GetMetadataAsync();

        // GET VIDEO STREAMS
        Task GetStreamsAsync();

        // DOWNLOAD VIDEO
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, Stream audioVideoStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, Stream audioVideoStream, MediaFileExtension extension, MediaType mediaType, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, IVStream videoStream, VideoFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, IVStream videoStream, VideoFileExtension extension, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, AudioFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IAStream audioStream, AudioFileExtension extension, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IVStream videoStream, VideoFileExtension extension, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default);
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IVStream videoStream, VideoFileExtension extension, CancellationToken cancellationToken = default);

        #endregion



        #region EVENT HANDLERS

        event EventHandler DownloadingStarted;

        event EventHandler<ProgressChangedEventArgs> DownloadingProgressChanged;

        event EventHandler DownloadingCompleted;

        event EventHandler ProcessingStarted;

        event EventHandler<ProgressChangedEventArgs> ProcessingProgressChanged;

        event EventHandler ProcessingCompleted;

        #endregion
    }
}
