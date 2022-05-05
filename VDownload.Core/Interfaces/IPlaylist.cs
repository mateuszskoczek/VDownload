using System;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;

namespace VDownload.Core.Interfaces
{
    public interface IPlaylist
    {
        #region PROPERTIES

        // PLAYLIST PROPERTIES
        string ID { get; }
        PlaylistSource Source { get; }
        Uri Url { get; }
        string Name { get; }
        IVideo[] Videos { get; }

        #endregion



        #region METHODS

        // GET PLAYLIST METADATA
        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        // GET VIDEOS FROM PLAYLIST
        Task GetVideosAsync(CancellationToken cancellationToken = default);
        Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default);

        #endregion
    }
}
