using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Sources.Common
{
    public class SearchRegexPlaylist : SearchRegex<Func<string, int, Task<Playlist>>>
    {
    }
}
