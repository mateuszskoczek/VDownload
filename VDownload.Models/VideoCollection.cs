using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public abstract class VideoCollection : List<Video>
    {
        #region PROPERTIES

        public required string Name { get; init; }
        public Source Source { get; protected set; }

        #endregion
    }
}
