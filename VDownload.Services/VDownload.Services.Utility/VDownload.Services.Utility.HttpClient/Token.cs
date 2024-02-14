using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Utility.HttpClient
{
    public class Token
    {
        #region PROPERTIES

        public string Schema { get; private set; }
        public byte[] TokenValue { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public Token(string schema, byte[] tokenValue)
        {
            Schema = schema;
            TokenValue = tokenValue;
        }

        #endregion



        #region PUBLIC METHODS

        public override string ToString() => $"{Schema} {Encoding.UTF8.GetString(TokenValue)}";

        #endregion
    }
}
