using VDownload.Core.Interfaces;

namespace VDownload.Core.EventArgs
{
    public class VideoSearchSuccessedEventArgs : System.EventArgs
    {
        public IVideo Video { get; set; }
    }
}
