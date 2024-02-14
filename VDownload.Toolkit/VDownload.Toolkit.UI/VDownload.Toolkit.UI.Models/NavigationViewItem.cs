using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Toolkit.UI.Models
{
    public class NavigationViewItem
    {
        #region PROPERTIES

        public string Name { get; set; }
        public string IconSource { get; set; }
        public Type ViewModel { get; set; }

        #endregion
    }
}
