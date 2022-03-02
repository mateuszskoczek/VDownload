using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using Windows.Storage;

namespace VDownload.Core.Interfaces
{
    public interface IVideoService
    {
        #region PROPERTIES

        // VIDEO PROPERTIES
        string ID { get; }
        Uri VideoUrl { get; }
        string Title { get; }
        string Author { get; }
        DateTime Date { get; }
        TimeSpan Duration { get; }
        long Views { get; }
        Uri Thumbnail { get; }
        IBaseStream[] BaseStreams { get; }

        #endregion



        #region METHODS

        // GET VIDEO METADATA
        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        // GET VIDEO STREAMS
        Task GetStreamsAsync(CancellationToken cancellationToken = default);

        // DOWNLOAD VIDEO
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, IBaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default);

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
