using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VDownload.Core.Sources;
using VDownload.Core.Structs;
using Windows.Storage;

namespace VDownload.Core.Services.Sources.Twitch.Helpers
{
    public static class Authorization
    {
        #region CONSTANTS

        public readonly static string ClientID = "yukkqkwp61wsv3u1pya17crpyaa98y";
        public readonly static string GQLApiClientID = "kimne78kx3ncx6brgo4mv6wki5h1ko";
        public readonly static Uri RedirectUrl = new Uri("https://www.vd.com");

        private readonly static string ResponseType = "token";
        private readonly static string[] Scopes = new[]
        { 
            "user:read:subscriptions",
        };
        public readonly static Uri AuthorizationUrl = new Uri($"https://id.twitch.tv/oauth2/authorize?client_id={ClientID}&redirect_uri={RedirectUrl.OriginalString}&response_type={ResponseType}&scope={string.Join(" ", Scopes)}");
        
        #endregion



        #region METHODS

        public static async Task<string> ReadAccessTokenAsync()
        {
            try
            {
                StorageFolder authDataFolder = await AuthorizationData.FolderLocation.GetFolderAsync(AuthorizationData.FolderName);
                StorageFile authDataFile = await authDataFolder.GetFileAsync($"Twitch.{AuthorizationData.FilesExtension}");

                return await FileIO.ReadTextAsync(authDataFile);
            }
            catch (FileNotFoundException) 
            {
                return null;
            }
        }

        public static async Task SaveAccessTokenAsync(string accessToken)
        {
            StorageFolder authDataFolder = await AuthorizationData.FolderLocation.CreateFolderAsync(AuthorizationData.FolderName, CreationCollisionOption.OpenIfExists);
            StorageFile authDataFile = await authDataFolder.CreateFileAsync($"Twitch.{AuthorizationData.FilesExtension}", CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteTextAsync(authDataFile, accessToken);
        }

        public static async Task DeleteAccessTokenAsync()
        {
            try
            {
                StorageFolder authDataFolder = await AuthorizationData.FolderLocation.GetFolderAsync(AuthorizationData.FolderName);
                StorageFile authDataFile = await authDataFolder.GetFileAsync($"Twitch.{AuthorizationData.FilesExtension}");

                await authDataFile.DeleteAsync();
            }
            catch (FileNotFoundException) { }
        } 

        public static async Task<TwitchAccessTokenValidationData> ValidateAccessTokenAsync(string accessToken)
        {
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };
            client.Headers.Add("Authorization", $"Bearer {accessToken}");

            try
            {
                JObject response = JObject.Parse(await client.DownloadStringTaskAsync("https://id.twitch.tv/oauth2/validate"));

                string login = response["login"].ToString();
                DateTime? expirationDate = DateTime.Now.AddSeconds(long.Parse(response["expires_in"].ToString()));

                return new TwitchAccessTokenValidationData(accessToken, true, login, expirationDate);
            }
            catch (WebException ex)
            {
                if (ex.Response is null)
                {
                    throw;
                }
                else
                {
                    JObject exInfo = JObject.Parse(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                    if ((int)exInfo["status"] == 401) return new TwitchAccessTokenValidationData(accessToken, false, null, null);
                    else throw;
                }
            }
        }

        public static async Task RevokeAccessTokenAsync(string accessToken)
        {
            WebClient client = new WebClient { Encoding = Encoding.UTF8 };

            await client.UploadStringTaskAsync(new Uri("https://id.twitch.tv/oauth2/revoke"), $"client_id={ClientID}&token={accessToken}");
        }

        #endregion
    }
}
