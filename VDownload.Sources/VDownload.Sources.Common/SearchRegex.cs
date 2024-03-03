using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Sources.Common
{
    public struct SearchRegex
    {
        public Regex Regex {  get; set; }
        public Func<string, Task<object>> SearchFunction { get; set; }
    }
}
