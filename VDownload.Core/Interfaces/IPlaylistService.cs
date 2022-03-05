using System;
using System.Threading;
using System.Threading.Tasks;

namespace VDownload.Core.Interfaces
{
    public interface IPlaylistService
    {
        #region PROPERTIES

        // PLAYLIST PROPERTIES
        string ID { get; }
        Uri PlaylistUrl { get; }
        string Name { get; }
        IVideoService[] Videos { get; }

        #endregion



        #region METHODS

        // GET PLAYLIST METADATA
        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        // GET VIDEOS FROM PLAYLIST
        Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default);

        #endregion
    }
}
