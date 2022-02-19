using System;

namespace VDownload.Core.EventArgsObjects
{
    public class PlaylistSearchEventArgs : EventArgs
    {
        public string Phrase { get; set; }
        public int Count { get; set; }
    }
}
