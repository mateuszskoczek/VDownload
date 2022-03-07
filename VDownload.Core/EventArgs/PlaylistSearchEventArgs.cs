namespace VDownload.Core.EventArgs
{
    public class PlaylistSearchEventArgs : System.EventArgs
    {
        public string Url { get; set; }
        public int VideosCount { get; set; }
    }
}
