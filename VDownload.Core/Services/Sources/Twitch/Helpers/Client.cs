using System.Net;
using System.Threading.Tasks;
using VDownload.Core.Exceptions;

namespace VDownload.Core.Services.Sources.Twitch.Helpers
{
    internal class Client
    {
        internal static async Task<WebClient> Helix()
        {
            // Get access token
            string accessToken = await Auth.ReadAccessTokenAsync();
            if (accessToken == null) throw new TwitchAccessTokenNotFoundException();

            // Check access token
            var twitchAccessTokenValidation = await Auth.ValidateAccessTokenAsync(accessToken);
            if (!twitchAccessTokenValidation.IsValid) throw new TwitchAccessTokenNotValidException();

            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Authorization", $"Bearer {accessToken}");
            client.Headers.Add("Client-Id", Auth.ClientID);

            // Return client
            return client;
        }

        internal static WebClient GQL()
        {
            // Create client
            WebClient client = new WebClient();
            client.Headers.Add("Client-Id", Auth.GQLApiClientID);

            // Return client
            return client;
        }
    }
}
