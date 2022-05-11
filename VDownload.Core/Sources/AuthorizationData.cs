using Windows.Storage;

namespace VDownload.Core.Sources
{
    internal static class AuthorizationData
    {
        internal static StorageFolder FolderLocation = ApplicationData.Current.LocalCacheFolder;
        internal static string FolderName = "AuthData";
        internal static string FilesExtension = "auth";
    }
}
