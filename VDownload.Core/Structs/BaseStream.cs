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
        public Uri Url { get; set; }
        public int Height { get; set; }
        public int FrameRate { get; set; }
    }
}
