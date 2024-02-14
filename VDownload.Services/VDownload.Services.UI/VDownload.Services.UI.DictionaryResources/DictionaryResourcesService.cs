using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.UI.DictionaryResources
{
    public interface IDictionaryResourcesService
    {
        T Get<T>(string key);
    }



    public class DictionaryResourcesService : IDictionaryResourcesService
    {
        #region CONSTRUCTORS

        public DictionaryResourcesService() { }

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
