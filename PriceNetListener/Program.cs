using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TIBCO.EMS;

namespace PriceNetListener
{
    class Program
    {
        static void Main(string[] args)
        {
            var _factory = new ConnectionFactory("tcp://vm-1af2-ae01.nam.nsroot.net:39422");
            var _conn = _factory.CreateConnection("speedit", "sp33dit");

            // create a session
            var _session = _conn.CreateSession(false, Session.AUTO_ACKNOWLEDGE);

            // create a listener
            //var _queueDestination = _session.CreateQueue("corp.cate.na.cloudapp");

            // create a listener, and start listening
            var _tempQueue = _session.CreateTemporaryQueue();


            var _queueReceiver = _session.CreateConsumer(_tempQueue);
            _queueReceiver.MessageHandler += (o, m) =>
            {
                Console.WriteLine("Received message: {0}", m.Message.ToString());
                _countdownEvent.Signal();
            };
            _conn.Start();

            // create a producer, and start producing
            var _queueSender = _session.CreateProducer(_tempQueue);
            _queueSender.DeliveryMode = DeliveryMode.RELIABLE_DELIVERY;

            for (int i = 0; i < _messageCount; i++)
            {
                var _mesg = _session.CreateTextMessage(string.Format("Hello, this is message {0}", i));
                _queueSender.Send(_mesg);
            }

        }
    }
}
