using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Services.ResourceDictionaries
{
    public interface IResourceDictionariesServices
    {
        #region PROPERTIES

        IImagesResourceDictionary Images { get; }

        #endregion



        #region METHODS

        T Get<T>(string key);

        #endregion
    }



    public class ResourceDictionariesServices : IResourceDictionariesServices
    {
        #region PROPERTIES

        public IImagesResourceDictionary Images { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public ResourceDictionariesServices(IImagesResourceDictionary imagesResourceDictionary)
        {
            Images = imagesResourceDictionary;
        }

        #endregion



        #region PUBLIC METHODS

        public T Get<T>(string key)
        {
            Application.Current.Resources.TryGetValue(key, out object value);
            if (value is not null && value is T cast)
            {
                return cast;
            }
            throw new KeyNotFoundException();
        }

        #endregion
    }
}
