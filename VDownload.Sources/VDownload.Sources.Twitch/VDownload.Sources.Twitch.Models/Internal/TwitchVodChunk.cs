using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Models.Internal
{
    internal class TwitchVodChunk
    {
        public required string Location { get; init; }
        public required string Url { get; init; }
        public required long Index { get; init; }
        public required TimeSpan Duration { get; init; }
    }
}
