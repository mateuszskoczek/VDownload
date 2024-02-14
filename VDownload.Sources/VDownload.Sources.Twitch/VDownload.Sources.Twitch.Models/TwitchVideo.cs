using VDownload.Models;

namespace VDownload.Sources.Twitch.Models
{
    public abstract class TwitchVideo : Video
    {
        #region CONSTRUCTORS

        protected TwitchVideo()
        {
            Source = Source.Twitch;
        }

        #endregion
    }
}
