using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public class VideoDownloadOptions
    {
        #region FIELDS

        private readonly TimeSpan _originalDuration;

        #endregion



        #region PROPERTIES

        public required VideoStream SelectedStream { get; set; }
        public required TimeSpan TrimStart { get; set; }
        public required TimeSpan TrimEnd { get; set; }
        public required MediaType MediaType { get; set; }
        public required string Directory { get; set; }
        public required string Filename { get; set; }
        public required string Extension { get; set; }
        public TimeSpan DurationAfterTrim => TrimEnd - TrimStart;
        public bool IsTrimmed => _originalDuration != DurationAfterTrim;

        #endregion



        #region CONSTRUCTORS

        public VideoDownloadOptions(TimeSpan originalDuration)
        {
            _originalDuration = originalDuration;
        }

        #endregion
    }
}
