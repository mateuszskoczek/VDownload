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

        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime PublishDate { get; set; }
        public TimeSpan Duration { get; set; }
        public long Views { get; set; }
        public Uri ThumbnailUrl { get; set; }
        public ICollection<VideoStream> Streams { get; set; }
        public Uri Url { get; set; }
        public Source Source { get; set; }

        #endregion



        #region CONSTRUCTORS

        protected Video() 
        {
            Streams = new List<VideoStream>();
        }

        #endregion
    }
}
