using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public abstract class Video
    {
        #region PROPERTIES

        public string Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime PublishDate { get; set; }
        public TimeSpan Duration { get; set; }
        public long Views { get; set; }
        public Uri? ThumbnailUrl { get; set; }
        public Uri Url { get; set; }
        public ICollection<VideoStream> Streams { get; private set; }
        public Source Source { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        protected Video() 
        {
            Streams = new List<VideoStream>();
        }

        #endregion
    }
}
