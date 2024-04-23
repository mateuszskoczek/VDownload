using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public struct VideoStreamDownloadResult
    {
        #region PROPERTIES

        public required string File { get; init; }
        public required TimeSpan NewTrimStart { get; init; }
        public required TimeSpan NewTrimEnd { get; init; }
        public required TimeSpan NewDuration { get; init; }

        #endregion
    }
}
