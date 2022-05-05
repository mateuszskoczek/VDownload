using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDownload.Core.Exceptions
{
    public class SubscriptionExistsException : Exception
    {
        public SubscriptionExistsException() { }
        public SubscriptionExistsException(string message) : base(message) { }
        public SubscriptionExistsException(string message, Exception inner) : base(message, inner) { }
    }
}
