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
    public class PublishClient : AbsPubSubClient
    {
        # region Public Properties
        public IMessagingProvider MessagingProvider { get; private set; }
        # endregion

        # region Constructor
        public PublishClient(XElement _configElement, ILog _logger)
        {
        }
        public PublishClient(IMessagingProvider _provider, ILog _logger) 
        {
            MessagingProvider = _provider;
        }
        # endregion

        # region Asynchronous Publish Methods
        public Task<bool> PublishAsync(string publishLocation, string replyLocation, string message, string clientID = null, bool autoAck = true, bool durable = false)
        {
            return PublishAsync<string>(publishLocation, replyLocation, message, clientID, autoAck, durable);
        }

        public Task<bool> PublishAsync<T>(string publishLocation, string replyLocation, T payload, string clientID = null, bool autoAck = true, bool durable = false)
        {
            return PublishAsync<T>(publishLocation, replyLocation, new T[] { payload }, clientID, autoAck, durable);
        }
        public Task<bool> PublishAsync<T>(string publishLocation, string replyLocation, IEnumerable<T> payload, string clientID = null, bool autoAck = true, bool durable = false)
        {
            return MessagingProvider.Publish<T>(publishLocation, replyLocation, payload, clientID, autoAck, durable);
        }

        # endregion
    }
}
