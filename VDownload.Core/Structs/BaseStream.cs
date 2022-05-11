using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Structs
{
    [Serializable]
    public struct BaseStream
    {
        #region PROPERTIES

        public Uri Url { get; set; }
        public int Height { get; set; }
        public int FrameRate { get; set; }

        #endregion



        #region METHODS

        public override string ToString() => $"{Height}p{(FrameRate > 0 ? FrameRate.ToString() : "N/A")}";

        #endregion
    }
}
