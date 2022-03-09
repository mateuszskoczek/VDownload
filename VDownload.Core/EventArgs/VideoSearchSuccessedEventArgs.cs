using VDownload.Core.Interfaces;

namespace VDownload.Core.EventArgs
{
    public class VideoSearchSuccessedEventArgs : System.EventArgs
    {
        public IVideoService VideoService { get; set; }
    }
}
