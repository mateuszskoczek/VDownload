using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Services.Data.Configuration;
using VDownload.Sources.Common;
using VDownload.Sources.Twitch.Api;
using VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Response;
using VDownload.Sources.Twitch.Api.Helix.GetVideos.Response;
using VDownload.Sources.Twitch.Authentication;
using VDownload.Sources.Twitch.Configuration.Models;
using VDownload.Sources.Twitch.Models;

namespace VDownload.Sources.Twitch
{
    public interface ITwitchSearchService : ISourceSearchService
    {
        Task<TwitchPlaylist> SearchPlaylist(string url, int maxVideoCount);
        Task<TwitchVideo> SearchVideo(string url);
    }



    public class TwitchSearchService : ITwitchSearchService
    {
        #region SERVICES

        protected readonly IConfigurationService _configurationService;
        protected readonly ITwitchApiService _apiService;
        protected readonly ITwitchAuthenticationService _twitchAuthenticationService;
        protected readonly ITwitchVideoStreamFactoryService _videoStreamFactoryService;

        #endregion



        #region CONSTRUCTORS

        public TwitchSearchService(IConfigurationService configurationService, ITwitchApiService apiService, ITwitchAuthenticationService authenticationService, ITwitchVideoStreamFactoryService videoStreamFactoryService)
        {
            _configurationService = configurationService;
            _apiService = apiService;
            _twitchAuthenticationService = authenticationService;
            _videoStreamFactoryService = videoStreamFactoryService;
        }

        #endregion



        #region PUBLIC METHODS

        async Task<Video> ISourceSearchService.SearchVideo(string url) => await SearchVideo(url);
        public async Task<TwitchVideo> SearchVideo(string url)
        {
            foreach (Regex regex in _configurationService.Twitch.Search.Vod.Regexes.Select(x => new Regex(x)))
            {
                Match match = regex.Match(url);
                if (match.Success)
                {
                    string id = match.Groups[1].Value;
                    return await GetVod(id);
                }
            }
            throw new MediaSearchException("Invalid url"); // TODO : Change to string resource
        }

        async Task<Playlist> ISourceSearchService.SearchPlaylist(string url, int maxVideoCount) => await SearchPlaylist(url, maxVideoCount);
        public async Task<TwitchPlaylist> SearchPlaylist(string url, int maxVideoCount)
        {
            throw new NotImplementedException();
        }

        #endregion



        #region PRIVATE METHODS

        protected async Task<TwitchVod> GetVod(string id)
        {
            Task<IEnumerable<TwitchVodStream>> streamsTask = GetVodStreams(id);

            byte[] token = await GetToken();
            GetVideosResponse info = await _apiService.HelixGetVideos(id, token);
            Data vodResponse = info.Data[0];

            Thumbnail thumbnail = _configurationService.Twitch.Search.Vod.Thumbnail;
            TwitchVod vod = new TwitchVod
            {
                Title = vodResponse.Title,
                Description = vodResponse.Description,
                Author = vodResponse.UserName,
                PublishDate = vodResponse.PublishedAt,
                Duration = ParseVodDuration(vodResponse.Duration),
                Views = vodResponse.ViewCount,
                ThumbnailUrl = new Uri(vodResponse.ThumbnailUrl.Replace("%{width}", thumbnail.Width.ToString()).Replace("%{height}", thumbnail.Height.ToString())),
                Url = new Uri(vodResponse.Url),
            };

            await streamsTask;
            foreach (TwitchVodStream stream in streamsTask.Result)
            {
                vod.Streams.Add(stream);
            }

            return vod;
        }

        protected async Task<IEnumerable<TwitchVodStream>> GetVodStreams(string id)
        {
            GetVideoTokenResponse videoToken = await _apiService.GQLGetVideoToken(id);

            string playlist = await _apiService.UsherGetVideoPlaylist(id, videoToken.Data.VideoPlaybackAccessToken.Value, videoToken.Data.VideoPlaybackAccessToken.Signature);

            Regex regex = new Regex(_configurationService.Twitch.Search.Vod.StreamPlaylistRegex);
            MatchCollection matches = regex.Matches(playlist);

            List<TwitchVodStream> streams = new List<TwitchVodStream>();
            foreach (Match match in matches)
            {
                TwitchVodStream stream = _videoStreamFactoryService.CreateVodStream();
                stream.Name = match.Groups["id"].Value;
                stream.VideoCodec = match.Groups["video_codec"].Value;
                stream.AudioCodec = match.Groups["audio_codec"].Value;
                stream.Width = int.Parse(match.Groups["width"].Value);
                stream.Height = int.Parse(match.Groups["height"].Value);
                stream.UrlM3U8 = match.Groups["url"].Value;
                streams.Add(stream);
            }
            return streams;
        }

        protected TimeSpan ParseVodDuration(string duration)
        {
            IEnumerable<string> parts = duration.Split(['h', 'm', 's'])[..^1].Reverse();
            string? seconds = parts.ElementAtOrDefault(0);
            string? minutes = parts.ElementAtOrDefault(1);
            string? hours = parts.ElementAtOrDefault(2);

            TimeSpan timeSpan = TimeSpan.Zero;
            if (!string.IsNullOrEmpty(seconds))
            {
                int secondsInt = int.Parse(seconds);
                timeSpan += TimeSpan.FromSeconds(secondsInt);
            }
            if (!string.IsNullOrEmpty(minutes))
            {
                int minutesInt = int.Parse(minutes);
                timeSpan += TimeSpan.FromMinutes(minutesInt);
            }
            if (!string.IsNullOrEmpty(hours))
            {
                int hoursInt = int.Parse(hours);
                timeSpan += TimeSpan.FromHours(hoursInt);
            }
            return timeSpan;
        }

        protected async Task<byte[]> GetToken()
        {
            byte[]? token = await _twitchAuthenticationService.GetToken();
            if (token is null)
            {
                throw new MediaSearchException("Not authenticated to Twitch"); // TODO : Change to string resource
            }
            TwitchValidationResult validation = await _twitchAuthenticationService.ValidateToken(token);
            if (!validation.Success)
            {
                throw new MediaSearchException("Twitch authentication error"); // TODO : Change to string resource
            }
            return token;
        }

        #endregion
    }
}
