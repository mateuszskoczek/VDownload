using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Common;
using VDownload.Common.Exceptions;
using VDownload.Common.Models;
using VDownload.Common.Services;
using VDownload.Services.HttpClient;
using VDownload.Sources.Twitch.Api;
using VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Response;
using VDownload.Sources.Twitch.Api.Helix.GetVideos.Response;
using VDownload.Sources.Twitch.Authentication;
using VDownload.Sources.Twitch.Configuration;

namespace VDownload.Sources.Twitch.Search
{
    public interface ITwitchSearchService : ISourceSearchService
    {
        Task<TwitchVideo> SearchVideo(string url);
        Task<TwitchPlaylist> SearchPlaylist(string url, int maxVideoCount);
    }

    public class TwitchSearchService : ITwitchSearchService
    {
        #region SERVICES

        private readonly TwitchApiHelixConfiguration _apiHelixConfiguration;
        private readonly TwitchApiGQLConfiguration _apiGQLConfiguration;
        private readonly TwitchSearchConfiguration _searchConfiguration;

        private readonly ITwitchApiService _apiService;
        private readonly ITwitchAuthenticationService _twitchAuthenticationService;

        #endregion



        #region CONSTRUCTORS

        public TwitchSearchService(TwitchConfiguration configuration, ITwitchApiService apiService, ITwitchAuthenticationService twitchAuthenticationService)
        {
            _apiHelixConfiguration = configuration.Api.Helix;
            _apiGQLConfiguration = configuration.Api.GQL;
            _searchConfiguration = configuration.Search;

            _apiService = apiService;
            _twitchAuthenticationService = twitchAuthenticationService;
        }

        #endregion



        #region PUBLIC METHODS

        async Task<Video> ISourceSearchService.SearchVideo(string url) => await SearchVideo(url);
        public async Task<TwitchVideo> SearchVideo(string url)
        {
            foreach (Regex regex in _searchConfiguration.VodRegexes)
            {
                Match match = regex.Match(url);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    return await GetVod(id);
                }
            }
            throw new MediaSearchException("Invalid url");
        }

        async Task<Playlist> ISourceSearchService.SearchPlaylist(string url, int maxVideoCount) => await SearchPlaylist(url, maxVideoCount);
        public async Task<TwitchPlaylist> SearchPlaylist(string url, int maxVideoCount)
        {
            throw new NotImplementedException();
        }

        #endregion



        #region PRIVATE METHODS

        private async Task<byte[]> GetToken()
        {
            byte[]? token = await _twitchAuthenticationService.GetToken();
            if (token is null)
            {
                throw new MediaSearchException("Not authenticated to Twitch");
            }
            TwitchValidationResult validation = await _twitchAuthenticationService.ValidateToken(token);
            if (!validation.Success)
            {
                throw new MediaSearchException("Twitch authentication error");
            }
            return token;
        }

        private async Task<TwitchVod> GetVod(string id)
        {
            Task<IEnumerable<TwitchVodStream>> streamsTask = GetVodStreams(id);

            byte[] token = await GetToken();
            GetVideosResponse info = await _apiService.HelixGetVideos(id, token);
            Data vodResponse = info.Data[0];

            TwitchVod vod = new TwitchVod
            {
                Title = vodResponse.Title,
                Description = vodResponse.Description,
                Author = vodResponse.UserName,
                PublishDate = vodResponse.PublishedAt,
                Duration = ParseVodDuration(vodResponse.Duration),
                ViewCount = vodResponse.ViewCount,
                ThumbnailUrl = vodResponse.ThumbnailUrl.Replace("%{width}", _searchConfiguration.VodThumbnailWidth.ToString()).Replace("%{height}", _searchConfiguration.VodThumbnailHeight.ToString()),
                Url = vodResponse.Url,
            };

            await streamsTask;
            foreach (TwitchVodStream stream in streamsTask.Result)
            {
                vod.Streams.Add(stream);
            }

            return vod;
        }

        private async Task<IEnumerable<TwitchVodStream>> GetVodStreams(string id)
        {
            GetVideoTokenResponse videoToken = await _apiService.GQLGetVideoToken(id);

            string playlist = await _apiService.UsherGetVideoPlaylist(id, videoToken.Data.VideoPlaybackAccessToken.Value, videoToken.Data.VideoPlaybackAccessToken.Signature);

            MatchCollection matches = _searchConfiguration.VodStreamPlaylistRegex.Matches(playlist);

            List<TwitchVodStream> streams = new List<TwitchVodStream>();
            foreach (Match match in matches)
            {
                streams.Add(new TwitchVodStream
                {
                    StreamIdentifier = match.Groups["id"].Value,
                    Codecs = match.Groups["codecs"].Value,
                    Width = int.Parse(match.Groups["width"].Value),
                    Height = int.Parse(match.Groups["height"].Value),
                    UrlM3U8 = match.Groups["url"].Value
                });
            }
            return streams;
        }

        private TimeSpan ParseVodDuration(string duration)
        {
            int hours = int.Parse(duration.Split('h')[0]);
            duration = duration.Split('h')[1];
            int minutes = int.Parse(duration.Split('m')[0]);
            duration = duration.Split('m')[1];
            int seconds = int.Parse(duration.Split('s')[0]);
            return TimeSpan.FromSeconds(seconds) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromHours(hours);
        }

        #endregion 
    }
}
