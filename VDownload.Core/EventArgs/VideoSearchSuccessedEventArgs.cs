using VDownload.Core.Interfaces;

namespace VDownload.Core.EventArgs
{
    public class VideoSearchSuccessedEventArgs : System.EventArgs
    {
        #region CONSTRUCTORS

        public VideoSearchSuccessedEventArgs(IVideo video)
        {
            Video = video;
        }

        #endregion



        #region PROPERTIES

        public IVideo Video { get; set; }

        #endregion
    }
}
