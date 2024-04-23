using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.UI.StoragePicker
{
    public class FileSavePickerFileTypeChoice
    {
        #region PROPERTIES

        public string Description { get; private set; }
        public IEnumerable<string> Extensions { get; private set; }

        #endregion



        #region CONSTRUCTORS

        public FileSavePickerFileTypeChoice(string description, string[] extensions)
        {
            Description = description;
            Extensions = extensions.Select(x => x.StartsWith('.') ? x : $".{x}");
        }

        #endregion
    }
}
