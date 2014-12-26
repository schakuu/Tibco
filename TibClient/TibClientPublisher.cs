using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Helper;
using Helper.Service;

using log4net;

namespace TibClient
{
    public class TibClientPublisher
    {
        public void Publish()
        {

            using (var _svcPool = new ServicePool(new XElement("test"), null))
            using (var _publishClient = _svcPool.PublishClient) 
            {
                _publishClient.Publish();
            }



        }
    }
}
