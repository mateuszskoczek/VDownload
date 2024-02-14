using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public enum MediaType
    {
        [Description("Original")]
        Original,

        [Description("Only video")]
        OnlyVideo,

        [Description("Only audio")]
        OnlyAudio,
    }
}
