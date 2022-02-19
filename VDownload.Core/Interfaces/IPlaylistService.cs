using System.Threading.Tasks;

namespace VDownload.Core.Interfaces
{
    internal interface IPlaylistService
    {
        #region PROPERTIES

        string ID { get; }
        string Name { get; }

        #endregion



        #region METHODS

        Task GetMetadataAsync();

        Task GetVideosAsync(int numberOfVideos);

        #endregion
    }
}
