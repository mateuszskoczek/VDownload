using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Services.ResourceDictionaries
{
    public interface IImagesResourceDictionary
    {
        // LOGO
        string Logo { get; }

        // SOURCES
        string SourcesTwitch { get; }

        // NAVIGATION VIEW
        string NavigationViewAuthentication { get; }
        string NavigationViewHome { get; }
    }



    public class ImagesResourceDictionary : IImagesResourceDictionary
    {
        #region PROPERTIES

        // LOGO
        public string Logo { get; private set; }

        // SOURCES
        public string SourcesTwitch { get; private set; }

        // NAVIGATION VIEW
        public string NavigationViewAuthentication { get; private set; }
        public string NavigationViewHome { get; private set; }


        #endregion



        #region CONSTRUCTORS

        public ImagesResourceDictionary()
        {
            Logo = (string)Application.Current.Resources["ImageLogo"];

            SourcesTwitch = (string)Application.Current.Resources["ImageSourcesTwitch"];

            NavigationViewAuthentication = (string)Application.Current.Resources["ImageNavigationViewAuthentication"];
            NavigationViewHome = (string)Application.Current.Resources["ImageNavigationViewHome"];
        }

        #endregion
    }
}
