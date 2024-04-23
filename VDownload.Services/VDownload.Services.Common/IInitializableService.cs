using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Services.Common
{
    public interface IInitializableService
    {
        #region METHODS

        Task Initialize();

        #endregion
    }

    public interface IInitializableService<T>
    {
        #region METHODS

        Task Initialize(T arg);

        #endregion
    }
}
