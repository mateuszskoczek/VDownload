using System.Threading.Tasks;
using Octokit;

namespace VDownload.Services
{
    class Update
    {
        #region CONSTANTS

        private static readonly string user = "VDownload"; // Username
        private static readonly string repositoryOwner = "mateuszskoczek"; // Repository owner
        private static readonly string repositoryName = "VDownload"; // Repository name
        private static readonly GitHubClient Client = new(new ProductHeaderValue(user)); // Github client

        #endregion CONSTANTS



        #region MAIN

        public static async Task<bool> Available()
        {
            // Get latest release
            var latestRelease = (await Client.Repository.Release.GetAll(repositoryOwner, repositoryName))[0];

            // Get build IDs
            float latestReleaseBuildID = float.Parse(latestRelease.Name.Split(' ')[1].Replace("(", "").Replace(")", "").Replace('.', ','));
            float installedReleaseBuildID = float.Parse(Global.ProgramInfo.BUILD_ID.Replace('.', ','));

            return (latestReleaseBuildID > installedReleaseBuildID);
        }

        #endregion MAIN
    }
}
