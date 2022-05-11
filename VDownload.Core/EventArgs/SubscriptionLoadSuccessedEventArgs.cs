using VDownload.Core.Interfaces;

namespace VDownload.Core.EventArgs
{
    public class SubscriptionLoadSuccessedEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public SubscriptionLoadSuccessedEventArgs(IVideo[] videos)
        {
            Videos = videos;
        }

        #endregion



        #region PROPERTIES

        public IVideo[] Videos { get; private set; }

        #endregion
    }
}
