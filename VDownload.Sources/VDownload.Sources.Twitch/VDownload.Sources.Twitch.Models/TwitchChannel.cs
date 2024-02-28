using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Models
{
    public class TwitchChannel : TwitchPlaylist
    {
        #region PROPERTIES

        public required string Id { get; set; }

        #endregion
    }
}
