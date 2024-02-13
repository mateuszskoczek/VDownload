using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Common.Models;

namespace VDownload.Common.Services
{
    public interface ISourceSearchService
    {
        Task<Video> SearchVideo(string url);
        Task<Playlist> SearchPlaylist(string url, int maxVideoCount);
    }
}
