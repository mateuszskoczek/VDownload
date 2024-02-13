using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.HttpClient;
using VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Response;
using VDownload.Sources.Twitch.Api.Helix.GetVideos.Response;
using VDownload.Sources.Twitch.Configuration;
using VDownload.Sources.Twitch.Search.Models.GetVideoToken.Request;

namespace VDownload.Sources.Twitch.Api
{
    public interface ITwitchApiService
    {
        Task<string> AuthValidate(byte[] token);
        Task<GetVideoTokenResponse> GQLGetVideoToken(string id);
        Task<GetVideosResponse> HelixGetVideos(string id, byte[] token);
        Task<string> UsherGetVideoPlaylist(string id, string videoToken, string videoTokenSignature);
    }



    public class TwitchApiService : ITwitchApiService
    {
        #region SERVICES

        private readonly TwitchApiAuthConfiguration _apiAuthConfiguration;
        private readonly TwitchApiHelixConfiguration _apiHelixConfiguration;
        private readonly TwitchApiGQLConfiguration _apiGQLConfiguration;
        private readonly TwitchApiUsherConfiguration _apiUsherConfiguration;

        private readonly IHttpClientService _httpClientService;

        #endregion



        #region CONSTRUCTORS

        public TwitchApiService(TwitchConfiguration configuration, IHttpClientService httpClientService)
        {
            _apiAuthConfiguration = configuration.Api.Auth;
            _apiHelixConfiguration = configuration.Api.Helix;
            _apiGQLConfiguration = configuration.Api.GQL;
            _apiUsherConfiguration = configuration.Api.Usher;

            _httpClientService = httpClientService;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<string> AuthValidate(byte[] token)
        {
            Token tokenData = new Token(_apiAuthConfiguration.TokenSchema, token);
            HttpRequest request = new HttpRequest(HttpMethodType.GET, _apiAuthConfiguration.Endpoints.Validate);
            request.Headers.Add("Authorization", $"{tokenData}");
            return await _httpClientService.SendRequestAsync(request);
        }

        public async Task<GetVideosResponse> HelixGetVideos(string id, byte[] token)
        {
            Token tokenData = new Token(_apiHelixConfiguration.TokenSchema, token);

            HttpRequest request = new HttpRequest(HttpMethodType.GET, _apiHelixConfiguration.Endpoints.GetVideos);
            request.Query.Add("id", id);
            request.Headers.Add("Authorization", tokenData.ToString());
            request.Headers.Add("Client-Id", _apiHelixConfiguration.ClientId);

            return await _httpClientService.SendRequestAsync<GetVideosResponse>(request);
        }

        public async Task<GetVideoTokenResponse> GQLGetVideoToken(string id)
        {
            TwitchApiGQLQueriesQueryExtendedConfiguration configuration = _apiGQLConfiguration.Queries.GetVideoToken;
            GetVideoTokenRequest requestBody = new GetVideoTokenRequest
            {
                OperationName = configuration.OperationName,
                Query = configuration.Query,
                Variables = new GetVideoTokenVariables
                {
                    IsLive = false,
                    Login = string.Empty,
                    IsVod = true,
                    VodID = id,
                    PlayerType = "embed"
                }
            };

            HttpRequest request = new HttpRequest(HttpMethodType.POST, _apiGQLConfiguration.Endpoint)
            {
                Body = requestBody,
            };
            request.Headers.Add("Client-Id", _apiGQLConfiguration.ClientId);
            return await _httpClientService.SendRequestAsync<GetVideoTokenResponse>(request);
        }

        public async Task<string> UsherGetVideoPlaylist(string id, string videoToken, string videoTokenSignature)
        {
            string url = string.Format(_apiUsherConfiguration.Endpoints.GetVideoPlaylist, id);
            HttpRequest request = new HttpRequest(HttpMethodType.GET, url);
            request.Query.Add("token", videoToken);
            request.Query.Add("sig", videoTokenSignature);
            request.Query.Add("allow_source", true);
            request.Query.Add("allow_audio_only", true);
            request.Query.Add("platform", "web");
            request.Query.Add("player_backend", "mediaplayer");
            request.Query.Add("playlist_include_framerate", true);
            request.Query.Add("supported_codecs", "av1,h264");
            return await _httpClientService.SendRequestAsync(request);
        }

        #endregion
    }
}
