using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Sources.Common
{
    public interface ISourceSearchService
    {
        Task<Video> SearchVideo(string url);
        Task<Playlist> SearchPlaylist(string url, int maxVideoCount);
    }



    public abstract class SourceSearchService : ISourceSearchService
    {
        #region PUBLIC METHODS

        public async Task<Video> SearchVideo(string url) 
        {
            foreach (SearchRegexVideo regex in GetVideoRegexes())
            {
                Match match = regex.Regex.Match(url);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    Video video = await regex.SearchFunction.Invoke(id);
                    return video;
                }
            }
            throw new MediaSearchException("Invalid url"); // TODO : Change to string resource
        }

        public async Task<Playlist> SearchPlaylist(string url, int maxVideoCount)
        {
            foreach (SearchRegexPlaylist regex in GetPlaylistRegexes())
            {
                Match match = regex.Regex.Match(url);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    Playlist video = await regex.SearchFunction.Invoke(id, maxVideoCount);
                    return video;
                }
            }
            throw new MediaSearchException("Invalid url"); // TODO : Change to string resource
        }

        #endregion



        #region PRIVATE METHODS

        protected abstract IEnumerable<SearchRegexVideo> GetVideoRegexes();

        protected abstract IEnumerable<SearchRegexPlaylist> GetPlaylistRegexes();

        #endregion
    }
}
