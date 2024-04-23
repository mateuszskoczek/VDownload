﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Configuration.Models;

namespace VDownload.Sources.Twitch.Api.GQL.GetClipToken.Response
{
    public class GetClipTokenData
    {
        [JsonProperty("clip")]
        public GetClipTokenClip Clip { get; set; }
    }
}
