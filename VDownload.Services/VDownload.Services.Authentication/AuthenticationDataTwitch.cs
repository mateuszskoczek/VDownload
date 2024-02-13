using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Authentication
{
    public class AuthenticationDataTwitch
    {
        [JsonProperty("token")]
        public byte[]? Token { get; set; }
    }
}
