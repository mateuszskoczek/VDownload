using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Interfaces
{
    internal interface IPlaylistService
    {
        #region PARAMETERS

        string ID { get; }
        string Name { get; }

        #endregion



        #region METHODS

        Task GetMetadataAsync();

        Task GetVideosAsync(int numberOfVideos);

        #endregion
    }
}
