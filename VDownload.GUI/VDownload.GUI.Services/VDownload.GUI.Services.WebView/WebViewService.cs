using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.GUI.Services.WebView
{
    public interface IWebViewService
    {
        Task<string> Show(Uri url, Predicate<string> closePredicate, string name);
    }



    public class WebViewService : IWebViewService
    {
        #region METHODS

        public async Task<string> Show(Uri url, Predicate<string> closePredicate, string name)
        {
            WebViewWindow window = new WebViewWindow(name);
            return await window.Show(url, closePredicate);
        }

        #endregion
    }
}
