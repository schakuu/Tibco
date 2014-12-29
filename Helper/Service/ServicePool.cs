using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using log4net;

using Helper.Messaging;

namespace Helper.Service
{
    public class ServicePool : IDisposable
    {
        # region Public Properties
        public PublishClient PublishClient { get; private set; }
        public SubscribeClient SubscribeClient { get; private set; }
        # endregion

        # region Constructor
        public ServicePool(XElement _configElement, ILog _logger)
        {

            // init the publish client
            PublishClient = new PublishClient(_configElement.Element("PublishClient"), _logger);

            // init the subscribe client
            SubscribeClient = new SubscribeClient(_configElement.Element("SubscribeClient"), _logger);

        }
        # endregion

        # region IDisposable        
        public void Dispose()
        {
        }
        # endregion
    }
}
