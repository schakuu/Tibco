using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using log4net;

using Helper.Messaging.Client;

namespace Helper.Messaging
{
    // callback delegate
    public delegate void CallbackDelegate(string sentToQueue, string message);

    public class SubscribeClient : AbsPubSubClient
    {
        # region Public Properties
        public IMessagingProvider MessagingProvider { get; private set; }
        # endregion

        # region Constructor
        public SubscribeClient(XElement _configElement, ILog _logger)
        {
        }
        public SubscribeClient(IMessagingProvider _provider, ILog _logger) 
        {
            MessagingProvider = _provider;
        }
        # endregion

        # region Subscription Methods
        public SubscriptionHandle SubscribeMessage(string subscribeQueue, CallbackDelegate callbackMethod)
        {
            return MessagingProvider.SubscribeMessage(subscribeQueue, callbackMethod);
        }
        public void Unsubscribe(SubscriptionHandle handle)
        {
            MessagingProvider.Unsubscribe(handle);
        }
        # endregion

    }
}
