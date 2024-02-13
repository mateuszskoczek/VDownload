using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Request;

namespace VDownload.Sources.Twitch.Api.GQL.GetVideoToken.Response
{
    public class GetVideoTokenData
    {
        [JsonProperty("videoPlaybackAccessToken")]
        public GetVideoTokenVideoPlaybackAccessToken VideoPlaybackAccessToken { get; set; }
    }
}
