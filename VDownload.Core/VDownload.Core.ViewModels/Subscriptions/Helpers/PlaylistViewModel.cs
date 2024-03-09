using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Models;

namespace VDownload.Core.ViewModels.Subscriptions.Helpers
{
    public partial class PlaylistViewModel : ObservableObject
    {
        #region PROPERTIES

        [ObservableProperty]
        protected Guid _guid;

        [ObservableProperty]
        protected string _name;

        [ObservableProperty]
        protected Source _source;

        #endregion



        #region CONSTRUCTORS

        public PlaylistViewModel(string name, Source source, Guid guid)
        {
            _name = name;
            _source = source;
            _guid = guid;
        }

        #endregion
    }
}
