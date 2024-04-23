using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Activation
{
    public interface IActivationHandler
    {
        bool CanHandle(object args);
        Task HandleAsync(object args);
    }



    public abstract class ActivationHandler<T> : IActivationHandler
    where T : class
    {
        #region PUBLIC METHODS

        public bool CanHandle(object args) => args is T && CanHandleInternal((args as T)!);

        public async Task HandleAsync(object args) => await HandleInternalAsync((args as T)!);

        #endregion



        #region PRIVATE METHODS

        protected virtual bool CanHandleInternal(T args) => true;

        protected abstract Task HandleInternalAsync(T args);

        #endregion
    }
}
