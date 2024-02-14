using VDownload.Services.Data.Configuration;
using VDownload.Services.Utility.HttpClient;
using VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Response;
using VDownload.Sources.Twitch.Api.Helix.GetVideos.Response;
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

        protected readonly IConfigurationService _configurationService;
        protected readonly IHttpClientService _httpClientService;

        #endregion



        #region CONSTRUCTORS

        public TwitchApiService(IConfigurationService configurationService, IHttpClientService httpClientService)
        {
            _configurationService = configurationService;
            _httpClientService = httpClientService;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<string> AuthValidate(byte[] token)
        {
            Token tokenData = new Token(_configurationService.Twitch.Api.Auth.TokenSchema, token);
            HttpRequest request = new HttpRequest(HttpMethodType.GET, _configurationService.Twitch.Api.Auth.Endpoints.Validate);
            request.Headers.Add("Authorization", $"{tokenData}");
            return await _httpClientService.SendRequestAsync(request);
        }

        public async Task<GetVideosResponse> HelixGetVideos(string id, byte[] token)
        {
            Token tokenData = new Token(_configurationService.Twitch.Api.Helix.TokenSchema, token);

            HttpRequest request = new HttpRequest(HttpMethodType.GET, _configurationService.Twitch.Api.Helix.Endpoints.GetVideos);
            request.Query.Add("id", id);
            request.Headers.Add("Authorization", tokenData.ToString());
            request.Headers.Add("Client-Id", _configurationService.Twitch.Api.Helix.ClientId);

            return await _httpClientService.SendRequestAsync<GetVideosResponse>(request);
        }

        public async Task<GetVideoTokenResponse> GQLGetVideoToken(string id)
        {
            GetVideoTokenRequest requestBody = new GetVideoTokenRequest
            {
                OperationName = _configurationService.Twitch.Api.Gql.Queries.GetVideoToken.OperationName,
                Query = _configurationService.Twitch.Api.Gql.Queries.GetVideoToken.Query,
                Variables = new GetVideoTokenVariables
                {
                    IsLive = false,
                    Login = string.Empty,
                    IsVod = true,
                    VodID = id,
                    PlayerType = "embed"
                }
            };

            HttpRequest request = new HttpRequest(HttpMethodType.POST, _configurationService.Twitch.Api.Gql.Endpoint)
            {
                Body = requestBody,
            };
            request.Headers.Add("Client-Id", _configurationService.Twitch.Api.Gql.ClientId);
            return await _httpClientService.SendRequestAsync<GetVideoTokenResponse>(request);
        }

        public async Task<string> UsherGetVideoPlaylist(string id, string videoToken, string videoTokenSignature)
        {
            string url = string.Format(_configurationService.Twitch.Api.Usher.Endpoints.GetVideoPlaylist, id);
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
