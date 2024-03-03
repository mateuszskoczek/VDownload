using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Sources.Common;
using VDownload.Sources.Twitch;
using VDownload.Sources.Twitch.Configuration;
using VDownload.Sources.Twitch.Search;

namespace VDownload.Sources
{
    public interface ISearchService
    {
        Task<Playlist> SearchPlaylist(string url, int maxVideoCount);
        Task<Video> SearchVideo(string url);
    }



    public class SearchService : ISearchService
    {
        #region FIELDS

        private readonly List<(Regex Regex, ISourceSearchService Service)> _urlMappings;

        #endregion



        #region CONSTRUCTORS

        public SearchService(IConfigurationService configurationService, ITwitchSearchService twitchSearchService)
        {
            _urlMappings =
            [
                .. configurationService.Twitch.Search.GeneralRegexes.Select(x => (new Regex(x), twitchSearchService)),
            ];
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<Video> SearchVideo(string url)
        {
            BaseUrlCheck(url);

            foreach ((Regex Regex, ISourceSearchService Service) mapping in _urlMappings)
            {
                if (mapping.Regex.IsMatch(url))
                {
                    return await mapping.Service.SearchVideo(url);
                }
            }

            throw CreateExceptionSourceNotSupported();
        }

        public async Task<Playlist> SearchPlaylist(string url, int maxVideoCount)
        {
            BaseUrlCheck(url);

            foreach ((Regex Regex, ISourceSearchService Service) mapping in _urlMappings)
            {
                if (mapping.Regex.IsMatch(url))
                {
                    return await mapping.Service.SearchPlaylist(url, maxVideoCount);
                }
            }

            throw CreateExceptionSourceNotSupported();
        }

        #endregion



        #region PRIVATE METHODS

        protected void BaseUrlCheck(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw CreateExceptionEmptyUrl();
            }
        }

        protected MediaSearchException CreateExceptionSourceNotSupported() => new MediaSearchException("SourceNotSupported");
        protected MediaSearchException CreateExceptionEmptyUrl() => new MediaSearchException("EmptyUrl");

        #endregion 
    }
}
