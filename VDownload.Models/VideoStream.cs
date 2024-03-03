using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public abstract class VideoStream
    {
        #region PROPERTIES

        public string Name { get; set; }

        #endregion



        #region PUBLIC METHODS

        public override string ToString() => Name;

        public abstract Task<VideoStreamDownloadResult> Download(string taskTemporaryDirectory, IProgress<double> onProgress, CancellationToken token, TimeSpan duration, TimeSpan trimStart, TimeSpan trimEnd);

        #endregion
    }
}
