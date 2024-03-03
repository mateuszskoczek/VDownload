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
using VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response;
using VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Response;
using VDownload.Sources.Twitch.Api.Helix.GetClips.Response;
using VDownload.Sources.Twitch.Api.Helix.GetUsers.Response;
using VDownload.Sources.Twitch.Api.Helix.GetVideos.Response;
using VDownload.Sources.Twitch.Authentication;
using VDownload.Sources.Twitch.Configuration.Models;
using VDownload.Sources.Twitch.Models;

namespace VDownload.Sources.Twitch
{
    public interface ITwitchSearchService : ISourceSearchService
    {
    }



    public class TwitchSearchService : SourceSearchService, ITwitchSearchService
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



        #region PRIVATE METHODS

        protected override IEnumerable<SearchRegexVideo> GetVideoRegexes()
        {
            return [
                .._configurationService.Twitch.Search.Vod.Regexes.Select(x => new SearchRegexVideo
                {
                    Regex = new Regex(x),
                    SearchFunction = async (id) => await GetVod(id)
                }),
                .._configurationService.Twitch.Search.Clip.Regexes.Select(x => new SearchRegexVideo
                {
                    Regex = new Regex(x),
                    SearchFunction = async (id) => await GetClip(id)
                }),
            ];
        }

        protected override IEnumerable<SearchRegexPlaylist> GetPlaylistRegexes()
        {
            return [
                .. _configurationService.Twitch.Search.Channel.Regexes.Select(x => new SearchRegexPlaylist
                {
                    Regex = new Regex(x),
                    SearchFunction = async (id, maxVideoCount) => await GetChannel(id, maxVideoCount)
                }),
            ];
        }

        protected async Task<TwitchVod> GetVod(string id)
        {
            byte[] token = await GetToken();

            GetVideosResponse info = await _apiService.HelixGetVideo(id, token);

            Api.Helix.GetVideos.Response.Data vodResponse = info.Data[0];

            TwitchVod vod = await ParseVod(vodResponse);

            return vod;
        }

        protected async Task<TwitchClip> GetClip(string id)
        {
            byte[] token = await GetToken();

            GetClipsResponse info = await _apiService.HelixGetClip(id, token);

            Api.Helix.GetClips.Response.Data clipResponse = info.Data[0];

            TwitchClip clip = await ParseClip(clipResponse);

            return clip;
        }

        protected async Task<TwitchChannel> GetChannel(string id, int count)
        {
            byte[] token = await GetToken();

            Api.Helix.GetUsers.Response.Data userResponse;
            try
            {
                GetUsersResponse info = await _apiService.HelixGetUser(id, token);
                if (info.Data.Count <= 0)
                {
                    throw CreateExceptionChannelNotFound();
                }
                userResponse = info.Data[0];
            }
            catch (InvalidOperationException ex)
            {
                // TODO: Add logging
                throw;
            }

            TwitchChannel channel = new TwitchChannel
            {
                Id = userResponse.Id,
                Name = userResponse.DisplayName,
                Description = userResponse.Description,
                Url = new Uri(string.Format(_configurationService.Twitch.Search.Channel.Url, id)),
            };

            List<Task<TwitchVod>> tasks = new List<Task<TwitchVod>>();
            string? cursor = null;
            List<Api.Helix.GetVideos.Response.Data> videosList;
            count = count == 0 ? int.MaxValue : count;
            int videos = 0;
            do
            {
                videos = count > 100 ? 100 : count;
                GetVideosResponse videosResponse = await _apiService.HelixGetUserVideos(channel.Id, token, videos, cursor);

                if (!tasks.Any() && !videosResponse.Data.Any())
                {
                    throw CreateExceptionEmptyPlaylist();
                }

                videosList = videosResponse.Data;
                cursor = videosResponse.Pagination.Cursor;
                tasks.AddRange(videosList.Select(ParseVod));
            }
            while (tasks.Count < count && videosList.Count == videos);

            await Task.WhenAll(tasks);

            channel.AddRange(tasks.Select(x => x.Result));

            return channel;
        }

        protected async Task<TwitchVod> ParseVod(Api.Helix.GetVideos.Response.Data data)
        {
            Task<IEnumerable<TwitchVodStream>> streamsTask = GetVodStreams(data.Id);

            Thumbnail thumbnailConfig = _configurationService.Twitch.Search.Vod.Thumbnail;
            Regex liveThumbnailRegex = new Regex(_configurationService.Twitch.Search.Vod.LiveThumbnailUrlRegex);
            Uri? thumbnail = null;
            if (!liveThumbnailRegex.IsMatch(data.ThumbnailUrl))
            {
                thumbnail = new Uri(data.ThumbnailUrl.Replace("%{width}", thumbnailConfig.Width.ToString()).Replace("%{height}", thumbnailConfig.Height.ToString()));
            }
            TwitchVod vod = new TwitchVod
            {
                Id = data.Id,
                Title = data.Title,
                Description = data.Description,
                Author = data.UserName,
                PublishDate = data.PublishedAt,
                Duration = ParseVodDuration(data.Duration),
                Views = data.ViewCount,
                ThumbnailUrl = thumbnail,
                Url = new Uri(data.Url),
            };

            await streamsTask;
            foreach (TwitchVodStream stream in streamsTask.Result)
            {
                vod.Streams.Add(stream);
            }

            return vod;
        }

        protected async Task<TwitchClip> ParseClip(Api.Helix.GetClips.Response.Data data)
        {
            Task<IEnumerable<TwitchClipStream>> streamsTask = GetClipStreams(data.Id);

            TwitchClip clip = new TwitchClip
            {
                Id = data.Id,
                Title = data.Title,
                Author = data.BroadcasterName,
                Creator = data.CreatorName,
                PublishDate = data.CreatedAt,
                Duration = TimeSpan.FromSeconds(Math.Round(data.Duration)),
                Views = data.ViewCount,
                ThumbnailUrl = new Uri(data.ThumbnailUrl),
                Url = new Uri(data.Url),
            };

            await streamsTask;
            foreach (TwitchClipStream stream in streamsTask.Result)
            {
                clip.Streams.Add(stream);
            }

            return clip;
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

        protected async Task<IEnumerable<TwitchClipStream>> GetClipStreams(string id)
        {
            GetClipTokenResponse clipToken = await _apiService.GQLGetClipToken(id);

            List<TwitchClipStream> streams = new List<TwitchClipStream>();
            foreach (GetClipTokenVideoQuality streamData in clipToken.Data.Clip.VideoQualities)
            {
                TwitchClipStream stream = _videoStreamFactoryService.CreateClipStream();
                stream.Name = $"{streamData.Quality}p{Math.Round(streamData.FrameRate)}";
                stream.Height = int.Parse(streamData.Quality);
                stream.FrameRate = streamData.FrameRate;
                stream.Url = new Uri(streamData.SourceURL);
                stream.Signature = clipToken.Data.Clip.PlaybackAccessToken.Signature;
                stream.Token = clipToken.Data.Clip.PlaybackAccessToken.Value;
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
            byte[]? token = await _twitchAuthenticationService.GetToken() ?? throw CreateExceptionNotAuthenticated();

            TwitchValidationResult validation = await _twitchAuthenticationService.ValidateToken(token);
            if (!validation.Success)
            {
                throw CreateExceptionTokenValidationUnsuccessful();
            }
            return token;
        }

        protected MediaSearchException CreateExceptionNotAuthenticated() => new MediaSearchException("TwitchNotAuthenticated");
        protected MediaSearchException CreateExceptionTokenValidationUnsuccessful() => new MediaSearchException("TwitchTokenValidationUnsuccessful");
        protected MediaSearchException CreateExceptionChannelNotFound() => new MediaSearchException("TwitchChannelNotFound");

        #endregion
    }
}
