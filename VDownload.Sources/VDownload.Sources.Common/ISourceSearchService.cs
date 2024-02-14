using VDownload.Models;

namespace VDownload.Sources.Common
{
    public interface ISourceSearchService
    {
        Task<Video> SearchVideo(string url);
        Task<Playlist> SearchPlaylist(string url, int maxVideoCount);
    }
}
