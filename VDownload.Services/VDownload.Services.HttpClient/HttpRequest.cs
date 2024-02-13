using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.HttpClient
{
    public class HttpRequest
    {
        #region PROPERTIES

        public HttpMethodType MethodType { get; private set; }
        public string Url { get; private set; }
        public Dictionary<string, string> Headers { get; private set; }
        public Dictionary<string, object> Query { get; private set; }
        public object? Body { get; set; }

        #endregion



        #region CONSTRUCTORS

        public HttpRequest(HttpMethodType methodType, string url)
        {
            MethodType = methodType;
            Url = url;
            Headers = new Dictionary<string, string>();
            Query = new Dictionary<string, object>();
        }

        #endregion
    }
}
