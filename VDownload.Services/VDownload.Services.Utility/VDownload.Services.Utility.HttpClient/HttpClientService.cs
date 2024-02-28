using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Reflection.PortableExecutable;
using System.Collections;

namespace VDownload.Services.Utility.HttpClient
{
    public interface IHttpClientService
    {
        Task<T?> SendRequestAsync<T>(HttpRequest request);
        Task<string> SendRequestAsync(HttpRequest request);
    }



    public class HttpClientService : IHttpClientService
    {
        #region SERVICES

        private readonly System.Net.Http.HttpClient _httpClient;

        #endregion



        #region CONSTRUCTORS

        public HttpClientService(System.Net.Http.HttpClient httpClient) 
        { 
            _httpClient = httpClient;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<T?> SendRequestAsync<T>(HttpRequest request) => JsonConvert.DeserializeObject<T>(await SendRequestAsync(request));
        public async Task<string> SendRequestAsync(HttpRequest request)
        {
            StringBuilder urlBuilder = new StringBuilder(request.Url);
            if (request.Query.Count > 0)
            {
                Dictionary<string, object> query = request.Query.ToDictionary();
                KeyValuePair<string, object> queryElement = query.ElementAt(0);
                query.Remove(queryElement.Key);

                urlBuilder.Append($"?{queryElement.Key}={queryElement.Value}");

                foreach (KeyValuePair<string, object> queryElementLoop in query)
                {
                    urlBuilder.Append($"&{queryElementLoop.Key}={queryElementLoop.Value}");
                }
            }

            HttpMethod method = request.MethodType switch
            {
                HttpMethodType.GET => HttpMethod.Get,
                HttpMethodType.POST => HttpMethod.Post,
                HttpMethodType.PUT => HttpMethod.Put,
                HttpMethodType.PATCH => HttpMethod.Patch,
                HttpMethodType.DELETE => HttpMethod.Delete,
            };

            HttpRequestMessage httpRequest = new HttpRequestMessage(method, urlBuilder.ToString());

            if (request.Body is not null)
            {
                string json = JsonConvert.SerializeObject(request.Body);
                HttpContent content = new StringContent(json);
                content.Headers.ContentType.MediaType = "application/json";
                httpRequest.Content = content;
            }
            
            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                httpRequest.Headers.Add(header.Key, header.Value);
            }

            HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);

            string responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        #endregion
    }
}
