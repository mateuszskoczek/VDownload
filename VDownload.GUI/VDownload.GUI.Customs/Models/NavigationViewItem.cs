using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Customs.Models
{
    public class NavigationViewItem 
    {
        #region PROPERTIES

        public required string Name { get; init; }
        public required string IconSource { get; init; }
        public required Type ViewModel { get; init; }

        #endregion
    }
}
