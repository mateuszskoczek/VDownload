using VDownload.Core.Interfaces;

namespace VDownload.Core.EventArgs
{
    public class PlaylistSearchSuccessedEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public PlaylistSearchSuccessedEventArgs(IPlaylist playlist)
        {
            Playlist = playlist;
        }

        #endregion



        #region PROPERTIES

        public IPlaylist Playlist { get; private set; }

        #endregion
    }
}
