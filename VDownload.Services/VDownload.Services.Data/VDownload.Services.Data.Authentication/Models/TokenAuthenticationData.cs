using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Data.Authentication.Models
{
    public class TokenAuthenticationData
    {
        [JsonProperty("token")]
        public byte[]? Token { get; set; }
    }
}
