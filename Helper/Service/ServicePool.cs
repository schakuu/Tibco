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
        # endregion

        # region Constructor
        public ServicePool(XElement _configElement, ILog _logger)
        {
        }
        # endregion



        # region IDisposable        
        public void Dispose()
        {
        }
        # endregion
    }
}
