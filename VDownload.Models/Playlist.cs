using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public abstract class Playlist : List<Video>
    {
        #region PROPERTIES

        public required string Name { get; set; }
        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public Source Source { get; protected set; }

        #endregion
    }
}
