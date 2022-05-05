using System.Net;
using System.Threading.Tasks;
using VDownload.Core.Exceptions;

namespace VDownload.Core.Services.Sources.Twitch.Helpers
{
    internal static class Client
    {
        internal static async Task<WebClient> Helix()
        {
            string accessToken = await Auth.ReadAccessTokenAsync();
            if (accessToken == null) throw new TwitchAccessTokenNotFoundException();

            var twitchAccessTokenValidation = await Auth.ValidateAccessTokenAsync(accessToken);
            if (!twitchAccessTokenValidation.IsValid) throw new TwitchAccessTokenNotValidException();

            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Bearer {accessToken}");
            client.Headers.Add("Client-Id", Auth.ClientID);

            return client;
        }

        internal static WebClient GQL()
        {
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Auth.GQLApiClientID);

            return client;
        }
    }
}
