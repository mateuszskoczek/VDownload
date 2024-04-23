using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Sources.Twitch.Models
{
    public abstract class TwitchPlaylist : Playlist
    {
        #region CONSTRUCTORS

        protected TwitchPlaylist()
        {
            Source = Source.Twitch;
        }

        #endregion
    }
}
