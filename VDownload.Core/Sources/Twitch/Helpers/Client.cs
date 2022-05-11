using System.Net;
using System.Threading.Tasks;
using VDownload.Core.Exceptions;

namespace VDownload.Core.Services.Sources.Twitch.Helpers
{
    internal static class Client
    {
        internal static async Task<WebClient> Helix()
        {
            string accessToken = await Authorization.ReadAccessTokenAsync();
            if (accessToken == null) throw new TwitchAccessTokenNotFoundException();

            var twitchAccessTokenValidation = await Authorization.ValidateAccessTokenAsync(accessToken);
            if (!twitchAccessTokenValidation.IsValid) throw new TwitchAccessTokenNotValidException();

            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Bearer {accessToken}");
            client.Headers.Add("Client-Id", Authorization.ClientID);

            return client;
        }

        internal static WebClient GQL()
        {
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Authorization.GQLApiClientID);

            return client;
        }
    }
}
