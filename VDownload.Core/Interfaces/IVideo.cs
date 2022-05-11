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

        VideoSource Source { get; }
        string ID { get; }
        Uri Url { get; }
        string Title { get; }
        string Author { get; }
        DateTime Date { get; }
        TimeSpan Duration { get; }
        long Views { get; }
        Uri Thumbnail { get; }
        BaseStream[] BaseStreams { get; }

        #endregion



        #region METHODS

        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        Task GetStreamsAsync(CancellationToken cancellationToken = default);

        Task<StorageFile> DownloadAndTranscodeAsync(StorageFolder downloadingFolder, BaseStream baseStream, MediaFileExtension extension, MediaType mediaType, TrimData trim, CancellationToken cancellationToken = default);

        #endregion



        #region EVENTS

        event EventHandler<EventArgs.ProgressChangedEventArgs> DownloadingProgressChanged;
        event EventHandler<EventArgs.ProgressChangedEventArgs> ProcessingProgressChanged;

        #endregion
    }
}
