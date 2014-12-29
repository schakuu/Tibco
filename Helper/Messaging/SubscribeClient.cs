using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using log4net;

namespace Helper.Messaging
{
    public class SubscribeClient : AbsPubSubClient
    {
        # region Constructor
        public SubscribeClient(XElement _configElement, ILog _logger)
        {

        }
        # endregion

        # region Public Delegate
        public delegate void CallbackDelegate(string sentToQueue, string message);
        # endregion

        # region Subscription Methods
        public SubscriptionHandle SubscribeMessage(string subscribeQueue, CallbackDelegate callbaclMethod)
        {

            return null;
        }

        public void Unscribe(SubscriptionHandle handle)
        {
        }
        # endregion

    }
}
