using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VDownload.Sources.Common
{
    public class SearchRegex<TFunc> where TFunc : Delegate
    {
        #region PROPERTIES

        public required Regex Regex { get; init; }

        public TFunc SearchFunction { get; set; }

        #endregion
    }
}
