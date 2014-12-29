using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using log4net;

namespace Helper.Messaging
{
    public class PublishClient : AbsPubSubClient
    {
        # region Constructor
        public PublishClient(XElement _configElement, ILog _logger)
        {

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
            return null;
        }
        # endregion
    }
}
