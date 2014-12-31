using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            using (var _cde = new CountdownEvent(1))
            using (var _subscribeClient = _svcPool.Wrapper.SubscribeClient)
            using (var _publishClient = _svcPool.Wrapper.PublishClient) 
            {
                _subscribeClient.SubscribeMessage("TestResponseQueue", 
                    (_q, _m) => 
                    {
                        // handle response

                        _cde.Signal();
                    });

                _publishClient.PublishMessage("TestRequestQueue", "TestResponseQueue", "Hi From TibClientPublisher");

                // block main thread till response is received
                _cde.Wait();
            }

        }
    }
}
