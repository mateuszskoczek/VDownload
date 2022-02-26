using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace VDownload.Core.Services.Sources.Twitch
{
    public class Auth
    {
        #region CONSTANTS

        // CLIENT ID
        public readonly static string ClientID = "yukkqkwp61wsv3u1pya17crpyaa98y";

        // GQL API CLIENT ID
        public readonly static string GQLApiClientID = "kimne78kx3ncx6brgo4mv6wki5h1ko";

        // REDIRECT URL
        public readonly static Uri RedirectUrl = new Uri("https://www.vd.com");

        // AUTHORIZATION URL
        private readonly static string ResponseType = "token";
        private readonly static string[] Scopes = new[]
        { 
            "user:read:subscriptions",
        };
        public readonly static Uri AuthorizationUrl = new Uri($"https://id.twitch.tv/oauth2/authorize?client_id={ClientID}&redirect_uri={RedirectUrl.OriginalString}&response_type={ResponseType}&scope={string.Join(" ", Scopes)}");
        
        #endregion



        #region METHODS

        // READ ACCESS TOKEN
        public static async Task<string> ReadAccessTokenAsync()
        {
            try
            {
                // Get file
                StorageFolder authDataFolder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("AuthData");
                StorageFile authDataFile = await authDataFolder.GetFileAsync("Twitch.auth");

                // Return data
                return await FileIO.ReadTextAsync(authDataFile);
            }
            catch (FileNotFoundException) 
            {
                return null;
            }
        }

        // SAVE ACCESS TOKEN
        public static async Task SaveAccessTokenAsync(string accessToken)
        {
            // Get file
            StorageFolder authDataFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("AuthData", CreationCollisionOption.OpenIfExists);
            StorageFile authDataFile = await authDataFolder.CreateFileAsync("Twitch.auth", CreationCollisionOption.ReplaceExisting);

            // Save data
            FileIO.WriteTextAsync(authDataFile, accessToken);
        }

        // DELETE ACCESS TOKEN
        public static async Task DeleteAccessTokenAsync()
        {
            try
            {
                // Get file
                StorageFolder authDataFolder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync("AuthData");
                StorageFile authDataFile = await authDataFolder.GetFileAsync("Twitch.auth");

                // Delete file
                await authDataFile.DeleteAsync();
            }
            catch (FileNotFoundException) { }
        } 

        // VALIDATE ACCESS TOKEN
        public static async Task<(bool IsValid, string Login, DateTime? ExpirationDate)> ValidateAccessTokenAsync(string accessToken)
        {
            // Create client
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            client.Headers.Add("Authorization", $"Bearer {accessToken}");

            try
            {
                // Check access token
                JObject response = JObject.Parse(await client.DownloadStringTaskAsync("https://id.twitch.tv/oauth2/validate"));

                string login = response["login"].ToString();
                DateTime? expirationDate = DateTime.Now.AddSeconds(long.Parse(response["expires_in"].ToString()));

                return (true, login, expirationDate);
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    JObject wexInfo = JObject.Parse(new StreamReader(wex.Response.GetResponseStream()).ReadToEnd());
                    if ((int)wexInfo["status"] == 401) return (false, null, null);
                    else throw;
                }
                else throw;
            }
        }

        // REVOKE ACCESS TOKEN
        public static async Task RevokeAccessTokenAsync(string accessToken)
        {
            // Create client
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };

            // Revoke access token
            await client.UploadStringTaskAsync(new Uri("https://id.twitch.tv/oauth2/revoke"), $"client_id={ClientID}&token={accessToken}");
        }

        #endregion
    }
}
