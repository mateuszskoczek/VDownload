using System.Threading;
using System.Threading.Tasks;

namespace VDownload.Core.Interfaces
{
    public interface IPlaylistService
    {
        #region PROPERTIES

        string ID { get; }
        string Name { get; }

        #endregion



        #region METHODS

        Task GetMetadataAsync(CancellationToken cancellationToken = default);

        Task GetVideosAsync(int numberOfVideos, CancellationToken cancellationToken = default);

        #endregion
    }
}
