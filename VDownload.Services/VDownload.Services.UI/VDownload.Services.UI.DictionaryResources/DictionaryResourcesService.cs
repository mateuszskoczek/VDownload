using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Services.Common;
using Windows.Media.Core;

namespace VDownload.Services.UI.DictionaryResources
{
    public interface IDictionaryResourcesService : IInitializableService<ResourceDictionary>
    {
        T Get<T>(string key);
    }



    public class DictionaryResourcesService : IDictionaryResourcesService
    {
        #region FIELDS

        protected ResourceDictionary _resources;

        #endregion



        #region PUBLIC METHODS

        public async Task Initialize(ResourceDictionary arg) => await Task.Run(() => _resources = arg);

        public T Get<T>(string key)
        {
            _resources.TryGetValue(key, out object value);
            if (value is not null && value is T cast)
            {
                return cast;
            }
            throw new KeyNotFoundException();
        }

        #endregion
    }
}
