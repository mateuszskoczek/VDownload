using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;
using VDownload.Sources.Twitch.Configuration.Models;

namespace VDownload.Core.ViewModels.Home
{
    public partial class HomePlaylistViewModel : ObservableObject
    {
        #region SERVICES



        #endregion



        #region FIELDS



        #endregion



        #region PROPERTIES

        [ObservableProperty]
        protected string _name;

        #endregion



        #region CONSTRUCTORS

        public HomePlaylistViewModel() 
        { 
        }

        #endregion



        #region PUBLIC METHODS

        public async Task LoadPlaylist(Playlist playlist)
        {
            Name = playlist.Name;
        }

        #endregion



        #region COMMANDS



        #endregion



        #region EVENTS

        public event EventHandler CloseRequested;

        #endregion
    }
}
