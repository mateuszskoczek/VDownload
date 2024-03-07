using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Models
{
    public class SubscriptionList : VideoCollection
    {
        #region CONSTRUCTORS

        public SubscriptionList()
        {
            Source = Source.Subscriptions;
        }

        #endregion
    }
}
