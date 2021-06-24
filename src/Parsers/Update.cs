using Octokit;

namespace VDownload.Parsers
{
    class Update
    {
        // CONSTANTS
        private static readonly string user = "VDownload";
        private static readonly GitHubClient Client = new(new ProductHeaderValue(user));

        // MAIN
        public static bool Available()
        {
            // Get latest release
            var latestRelease = Client.Repository.Release.GetAll("mateuszskoczek", "VDownload").Result[0];

            // Get build IDs
            float latestReleaseBuildID = float.Parse(latestRelease.Name.Split(' ')[1].Replace("(", "").Replace(")", "").Replace('.', ','));
            float installedReleaseBuildID = float.Parse(Global.ProgramInfo.BUILD_ID.Replace('.', ','));

            return (latestReleaseBuildID > installedReleaseBuildID);
        }
    }
}
