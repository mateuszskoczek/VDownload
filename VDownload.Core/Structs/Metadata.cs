using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Structs
{
    public struct Metadata
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
        public long Views { get; set; }
        public Uri Thumbnail { get; set; }
    }
}
