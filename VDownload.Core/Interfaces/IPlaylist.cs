using System;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Enums;

namespace VDownload.Core.Interfaces
{
    public interface IPlaylist
    {
        #region PROPERTIES

        PlaylistSource Source { get; }
        string ID { get; }
        Uri Url { get; }
        string Name { get; }
        IVideo[] Videos { get; }

        #endregion



        #region METHODS

        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        Task GetVideosAsync(CancellationToken cancellationToken = default);
        Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default);

        #endregion
    }
}
