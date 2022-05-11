using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VDownload.Core.Interfaces;

namespace VDownload.Core.Services
{
    [Serializable]
    public class Subscription
    {
        #region CONSTRUCTORS

        public Subscription(IPlaylist playlist)
        {
            Playlist = playlist;
            SavedVideos = Playlist.Videos;
        }

        #endregion



        #region PROPERTIES

        public IPlaylist Playlist { get; private set; }
        public IVideo[] SavedVideos { get; private set; }

        #endregion



        #region PUBLIC METHODS

        public async Task<IVideo[]> GetNewVideosAsync(CancellationToken cancellationToken = default)
        {
            await Playlist.GetVideosAsync(cancellationToken);
            return GetUnsavedVideos();
        }

        public async Task<IVideo[]> GetNewVideosAndUpdateAsync(CancellationToken cancellationToken = default)
        {
            await Playlist.GetVideosAsync(cancellationToken);
            IVideo[] newVideos = GetUnsavedVideos();
            SavedVideos = Playlist.Videos;
            return newVideos;
        }

        #endregion



        #region PRIVATE METHODS

        private IVideo[] GetUnsavedVideos()
        {
            List<IVideo> newVideos = Playlist.Videos.ToList();
            foreach (IVideo savedVideo in SavedVideos)
            {
                newVideos.RemoveAll((v) => v.Source == savedVideo.Source && v.ID == savedVideo.ID);
            }
            return newVideos.ToArray();
        }

        #endregion
    }
}
