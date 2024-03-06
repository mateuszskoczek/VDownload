using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.ViewModels.About.Helpers
{
    public partial class PersonViewModel : ObservableObject
    {
        #region PROPERTIES

        [ObservableProperty]
        protected string _name;

        [ObservableProperty]
        protected Uri _url;

        #endregion



        #region CONSTRUCTORS

        public PersonViewModel(string name, string url)
        {
            _name = name;
            _url = new Uri(url);
        }

        #endregion
    }
}
