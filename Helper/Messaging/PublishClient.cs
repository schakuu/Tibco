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

        # region Synchronous Publish Methods
        public bool PublishMessage(string publishQueue, string replyQueue, string message)
        {
            return true;
        }

        # endregion

        # region Asynchronous Publish Methods
        public Task<bool> PublishMessageAsync(string publishQueue, string replyQueue, string message)
        {
            return MessagingProvider.PublishMessage(publishQueue, replyQueue, message);
        }
        # endregion
    }
}
