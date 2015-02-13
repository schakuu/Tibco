using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Helper.Service;

using log4net;

namespace TibClient
{
    public class TibClientPublisher
    {
        public void Publish(Dictionary<string, string> _properties)
        {

            //var _properties = new Dictionary<string, string>()
            //{
            //    {"assembly", null},
            //    {"class", "Helper.Messaging.Client.RabbitMQProvider"},
            //    {"host", "mktdevpc3606"},
            //    {"port", "5672"},
            //    {"endpoint", "amqp://mktdevpc3606:5672"},
            //    {"user", "speed1"},
            //    {"pwd", "spdpwd1"}
            //};

            //// publish and subscribe
            //using (var _svcPool = new ServicePool(_properties, null))
            //using (var _cde = new CountdownEvent(1))
            //using (var _subscribeClient = _svcPool.Wrapper.SubscribeClient)
            //using (var _publishClient = _svcPool.Wrapper.PublishClient) 
            //{
            //    var _handle = _subscribeClient.SubscribeMessage("citi.gsm.na.speed_156762.thetica.citimodel.response",
            //        (_q, _m) =>
            //        {
            //            Console.WriteLine(_m);
            //        });
            //    for (var _i = 0; _i < 20; _i++ )
            //        _publishClient.PublishMessageAsync("citi.gsm.na.speed_156762.thetica.citimodel.request", "citi.gsm.na.speed_156762.thetica.citimodel.response", string.Format("Request{0}", _i));

            //    // block main thread till response is received
            //    _cde.Wait();
            //    //_subscribeClient.Unsubscribe(_handle);
            //}

            // subscribe only
            using (var _svcPool = new ServicePool(_properties, null))
            using (var _autoReset = new CountdownEvent(20))
            using (var _subscribeClient = _svcPool.Wrapper.SubscribeClient)
            using (var _publishClient = _svcPool.Wrapper.PublishClient)
            {
                var _handle = _subscribeClient.SubscribeMessage("citi.gsm.na.speed_156762.ste.testqueue",
                    (_q, _m) =>
                    {
                        Console.WriteLine(_m);
                        _autoReset.Signal();
                    });

                _autoReset.Wait();
                _subscribeClient.Unsubscribe(_handle);

                //// publish ONLY
                //// create messages
                //string[] _messages = new string[10];
                //for (var _i = 0; _i < 10; _i++)
                //    _messages[_i] = string.Format("Request{0}", _i);

                //var _pubTask = _publishClient.PublishAsync<string>("citi.gsm.na.speed_156762.ste.testqueue", null, _messages);
                //Console.WriteLine("Waiting for publish to complete");
                //Task.WaitAll(_pubTask);
                //Console.WriteLine("Publishing is complete");
            }

        }
    }
}
