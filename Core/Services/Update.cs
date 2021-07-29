// System
using System.Collections.Generic;
using System.Threading.Tasks;

// External
using Octokit;



namespace VDownload.Core.Services
{
    class Update
    {
        #region CONSTANTS

        private static readonly string User = "VDownload"; // USERNAME
        private static readonly string RepositoryOwner = "mateuszskoczek"; // REPOSITORY OWNER
        private static readonly string RepositoryName = "VDownload"; // REPOSITORY NAME
        private static readonly GitHubClient Client = new(new ProductHeaderValue(User)); // GITHUB CLIENT

        #endregion



        #region MAIN

        // GET BUILD ID OF LATEST RELEASE
        public static async Task<string> LatestBuildID()
        {
            return (await LatestRelease()).Name.Split(' ')[1].Replace("(", "").Replace(")", "");
        }


        // GET VERSION OF LATEST RELEASE
        public static async Task<string> LatestVersion()
        {
            return (await LatestRelease()).Name.Split(' ')[0];
        }


        // GET URL OF LATEST RELEASE
        public static async Task<string> LatestUrl()
        {
            return (await LatestRelease()).HtmlUrl;
        }


        // CHECK IF UPDATE IS AVAILABLE
        public static async Task<bool> Available()
        {
            return float.Parse((await LatestBuildID()).Replace('.', ',')) > float.Parse(Globals.Info.Build.Replace('.', ','));
        }

        #endregion



        #region OTHER

        // GET ALL RELEASES
        private static async Task<IReadOnlyList<Release>> Releases()
        {
            return await Client.Repository.Release.GetAll(RepositoryOwner, RepositoryName);
        }


        // GET LATEST RELEASE
        private static async Task<Release> LatestRelease()
        {
            return (await Releases())[0];
        }

        #endregion
    }
}
