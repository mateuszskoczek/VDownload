using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Common
{
    public class MediaSearchException : Exception
    {
        #region PROPERTIES

        public string StringCode { get; protected set; }

        #endregion



        #region CONSTRUCTORS

        public MediaSearchException(string stringCode) : this(stringCode, stringCode) { }
        public MediaSearchException(string stringCode, string message) : base(message)
        {
            StringCode = stringCode;
        }

        #endregion
    }
}
