using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Sources.Common
{
    public class SearchRegexVideo : SearchRegex<Func<string, Task<Video>>>
    {
    }
}
