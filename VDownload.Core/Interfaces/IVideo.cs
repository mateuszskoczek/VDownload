using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;
using VDownload.Core.Structs;
using Windows.Storage;

namespace VDownload.Core.Interfaces
{
    public interface IVideo
    {
        #region PROPERTIES

        // VIDEO PROPERTIES
        VideoSource Source { get; }
        string ID { get; }
        Uri Url { get; }
        Metadata Metadata { get; }
        BaseStream[] BaseStreams { get; }

        #endregion



        #region METHODS

        // GET VIDEO METADATA
        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        // GET VIDEO STREAMS
        Task GetStreamsAsync(CancellationToken cancellationToken = default);

        // DOWNLOAD VIDEO
        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, BaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TimeSpan trimStart, TimeSpan trimEnd, CancellationToken cancellationToken = default);

        #endregion



        #region EVENT HANDLERS

        event EventHandler<EventArgs.ProgressChangedEventArgs> DownloadingProgressChanged;
        event EventHandler<EventArgs.ProgressChangedEventArgs> ProcessingProgressChanged;

        #endregion
    }
}
