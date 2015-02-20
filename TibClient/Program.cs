using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using TIBCO.EMS;

namespace TibClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // test - set some of the properties - DEV
            //var _properties = new Dictionary<string, string>() 
            //{ 
            //    { "assembly", null }, 
            //    { "class", "Helper.Messaging.Client.TibcoEMSProvider" },
            //    { "server1", "tcp://icgemsmw01d.nam.nsroot.net:7222,tcp://icgemsmw02d.nam.nsroot.net:7222"},
            //    { "server", "tcp://icgesbdev.nam.nsroot.net:7222"},
            //    { "username", "speed_156762"},
            //    { "password", "speed_156762pwd"},
            //    { "connectionfactory", "citi.gsm.na.speed_156762.GenericCF"}
            //};

            // test - set some of the properties - UAT
            var _properties = new Dictionary<string, string>() 
            { 
                { "assembly", null }, 
                { "class", "Helper.Messaging.Client.TibcoEMSProvider" },
                { "server", "ssl://icgemsmw01u.nam.nsroot.net:7243, ssl://icgemsmw02u.nam.nsroot.net:7243"},
                { "sslidentity", "speed_156762_uat.p12"},
                { "sslpassword", "soa123"},
                { "ssltargethostname", "icgems_tibesb_jndi_certauth_steesb_na_uat"},
                { "connectionfactory", "citi.gsm.na.speed_156762.GenericCF"},
                { "username", "speed_156762"},
                { "password", "speed_156762pwd"}
            };

            if (args[0].Equals("simplepublish", StringComparison.CurrentCultureIgnoreCase))
                new TibSimplePublish().Publish(_properties, args[1]);

            if (args[0].Equals("simpleconsume", StringComparison.CurrentCultureIgnoreCase))
                new TibSimpleConsume().Consume(_properties, args[1]);

            if (args[0].Equals("echo", StringComparison.CurrentCultureIgnoreCase))
                new TibClientEcho().Echo(_properties, args[1]);

            if (args[0].Equals("pubsub", StringComparison.CurrentCultureIgnoreCase))
                new TibPublishSubscribe().Start(_properties, args[1], args[2]);

            if (args[0].Equals("lookup", StringComparison.CurrentCultureIgnoreCase))
                new TibLookup().Lookup(_properties, args[1]);

            if (args[0].Equals("browser", StringComparison.CurrentCultureIgnoreCase))
                new TibBrowser().Browse();

            if (args[0].Equals("helper"))
                new TibClientPublisher().Publish(_properties);

            //////////////////////////////////////////////////////////////////////////////////
            //          OLD CODE                                                            //
            //////////////////////////////////////////////////////////////////////////////////

            //// start
            //var _messageCount = 20;
            ////var _factory = new ConnectionFactory("tcp://vm-1af2-ae01.nam.nsroot.net:39422");
            ////var _conn = _factory.CreateConnection("speedit", "sp33dit");

            //// DEV STE
            //var _factory = new ConnectionFactory("tcp://icgesbdev.nam.nsroot.net:7222");
            //var _conn = _factory.CreateConnection();

            //var _countdownEvent = new CountdownEvent(_messageCount);

            //// create a session
            //var _session = _conn.CreateSession(false, Session.CLIENT_ACKNOWLEDGE);

            //// create a listener
            ////var _queueDestination = _session.CreateQueue("corp.cate.na.cloudapp");

            //// create a listener, and start listening
            //var _tempReqQueue = _session.CreateQueue("citi.gsm.na.speed_156762.thetica.citimodel.request");
            //var _tempResQueue = _session.CreateQueue("citi.gsm.na.speed_156762.thetica.analytics.response");

            //var _queueReceiver = _session.CreateConsumer(_tempResQueue);
            //_queueReceiver.MessageHandler += (o, m) =>
            //{
            //    Console.WriteLine("Received message: {0}", m.Message.ToString());
            //    _countdownEvent.Signal();
            //};
            //_conn.Start();

            //// create a producer, and start producing
            //var _queueSender = _session.CreateProducer(_tempReqQueue);
            //_queueSender.DeliveryMode = DeliveryMode.RELIABLE_DELIVERY;

            //for (int i = 0; i < _messageCount; i++)
            //{
            //    var _mesg = _session.CreateTextMessage(string.Format("Hello, this is message {0}", i));
            //    _queueSender.Send(_mesg);
            //}

            //// wait for all messages to be received
            //_countdownEvent.Wait();
        }
    }
}
