using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Sources.Twitch.Models
{
    public class TwitchClip : TwitchVideo
    {
        #region PROPERTIES

        public string Creator { get; set; }

        #endregion
    }
}
