using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public abstract class Playlist : VideoCollection
    {
        #region PROPERTIES

        public required string Description { get; set; }
        public required Uri Url { get; set; }
        public Source Source { get; protected set; }

        #endregion
    }
}
