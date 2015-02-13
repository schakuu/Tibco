using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Messaging
{
    public class SubscriptionHandle
    {
        public string SubscribedToQueue { get; set; }
        public string SubscriptionIdentifier { get; set; }

        // this is a provider specific handle, that corresponds to the specific subscription
        public object Handle { get; set; }
    }
}
