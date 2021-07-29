// System
using System.Collections.Generic;

// External
using ConsoleTableExt;



namespace VDownload.Core.Globals
{
    class Table
    {
        public static readonly Dictionary<HeaderCharMapPositions, char> Streams = new()
        {
            { HeaderCharMapPositions.TopLeft, '╒' },
            { HeaderCharMapPositions.TopCenter, '╤' },
            { HeaderCharMapPositions.TopRight, '╕' },
            { HeaderCharMapPositions.BottomLeft, '╞' },
            { HeaderCharMapPositions.BottomCenter, '╪' },
            { HeaderCharMapPositions.BottomRight, '╡' },
            { HeaderCharMapPositions.BorderTop, '═' },
            { HeaderCharMapPositions.BorderRight, '│' },
            { HeaderCharMapPositions.BorderBottom, '═' },
            { HeaderCharMapPositions.BorderLeft, '│' },
            { HeaderCharMapPositions.Divider, '│' },
        };
    }
}
