using Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace VDownload.Core.Strings
{
    public class StringResource
    {
        #region FIELDS

        protected ResourceLoader _resourceLoader;

        #endregion



        #region CONSTRUCTORS

        internal StringResource(ResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        #endregion



        #region PUBLIC METHODS

        public string Get(string key)
        {
            return _resourceLoader.GetString(key);
        }

        #endregion
    }
}
