using Microsoft.Windows.ApplicationModel.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Get(string key) => _resourceLoader.GetString(key);

        #endregion
    }
}
