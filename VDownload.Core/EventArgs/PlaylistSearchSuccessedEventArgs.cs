using VDownload.Core.Interfaces;

namespace VDownload.Core.EventArgs
{
    public class PlaylistSearchSuccessedEventArgs : System.EventArgs
    {
        public IPlaylistService PlaylistService { get; set; }
    }
}
