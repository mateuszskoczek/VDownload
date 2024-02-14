using Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.UI.StringResources
{
    public class StringResources
    {
        #region FIELDS

        protected ResourceLoader _resourceLoader;

        #endregion



        #region CONSTRUCTORS

        internal StringResources(ResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        #endregion



        #region PUBLIC METHODS

        public string Get(string key) => _resourceLoader.GetString(key);

        #endregion
    }
}
