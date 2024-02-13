using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Common.Models;

namespace VDownload.Sources.Twitch
{
    public abstract class TwitchVideo : Video
    {
        #region CONSTRUCTORS

        protected TwitchVideo() 
        {
            _source = Common.Source.Twitch;
        }

        #endregion
    }
}
