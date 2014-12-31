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
    public class PubSubWrapper : IDisposable
    {
        # region Public properties
        public IMessagingProvider MessagingProvider { get; private set; }
        public PublishClient PublishClient { get; private set; }
        public SubscribeClient SubscribeClient { get; private set; }
        # endregion

        # region Constructor
        public PubSubWrapper(XElement _configElement, ILog _logger)
        {
            // init the messaging provider
            MessagingProvider = (IMessagingProvider)ObjectFactory.Create(
                _configElement.Attribute("assembly").Value,
                _configElement.Attribute("class").Value,
                _configElement);

            // init the publish client
            PublishClient = new PublishClient(MessagingProvider, _logger);

            // init the subscribe client
            SubscribeClient = new SubscribeClient(MessagingProvider, _logger);
        }

        public PubSubWrapper(IDictionary<string, string> _configProperties, ILog _logger)
        {
            MessagingProvider = (IMessagingProvider)ObjectFactory.Create(
                _configProperties["assembly"],
                _configProperties["class"],
                _configProperties.Values.ToArray());

            // init the publish client
            PublishClient = new PublishClient(MessagingProvider, _logger);

            // init the subscribe client
            SubscribeClient = new SubscribeClient(MessagingProvider, _logger);

        }
        # endregion

        # region Disposable 
        public void Dispose()
        {

        }
        # endregion
    }
}
