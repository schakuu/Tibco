using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using TIBCO.EMS;

namespace Helper.Messaging.Client
{
    public class TibcoEMSProvider : AbsMessagingProvider
    {
        # region Private Properties
        private string _connectionFactoryName;
        private string _server;
        private TibcoEMSLookup _lookup;
        ConnectionFactory ConnectionFactory { get; set; }
        ConcurrentDictionary<long, Session> CurrentConsumerSessions = new ConcurrentDictionary<long, Session>();
        # endregion

        # region Constructor
        public TibcoEMSProvider(IDictionary<string, string> configProperties)
        {
            _connectionFactoryName = configProperties["connectionfactory"];
            _server = configProperties["server"];
            _lookup = new TibcoEMSLookup(configProperties);
        }
        public TibcoEMSProvider(XElement configElement)
        {
            _lookup = new TibcoEMSLookup(configElement);
        }
        # endregion

        # region Init
        protected override void InitProvider()
        {
            ConnectionFactory = _lookup.LookupConnectionFactory(_connectionFactoryName);
            //ConnectionFactory = new ConnectionFactory(_server);
        }
        # endregion

        # region Publish
        protected override Task<bool> HandlePublishMessage(string publishQueue, string replyToQueue, string message, bool autoAck = false)
        {

            var _taskSource = new TaskCompletionSource<bool>();
            var _task = _taskSource.Task;

            SessionMode _ackMode = autoAck ? SessionMode.AutoAcknowledge : SessionMode.ExplicitClientAcknowledge;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    // connection
                    var _connection = ConnectionFactory.CreateConnection();
                    _connection.Start();

                    _taskSource.Task.ContinueWith(_t => 
                    {
                        if (_t.IsCompleted)
                        {
                            if (_connection != null && !_connection.IsClosed)
                                _connection.Close();
                        }
                    });

                    // non-transacted session
                    var _session = _connection.CreateSession(false, _ackMode);
                    var _destination = _lookup.LookupDestination(publishQueue); // _session.CreateQueue(publishQueue); // handle topic

                    // create the message producer -- TODO set some of producer properties
                    var _producer = _session.CreateProducer(null);
                    _producer.MsgDeliveryMode = MessageDeliveryMode.ReliableDelivery;
                    _producer.DeliveryMode = DeliveryMode.RELIABLE_DELIVERY;

                    // message
                    var _mesg = _session.CreateTextMessage(message);
                    if (!string.IsNullOrWhiteSpace(replyToQueue))
                        _mesg.ReplyTo = _lookup.LookupDestination(replyToQueue);  //_session.CreateQueue(replyToQueue);
                    _producer.Send(_destination, _mesg);

                    _taskSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    _taskSource.TrySetException(ex);
                }
            }).ContinueWith(_t =>
            {
                if (_t.Status.Equals(TaskStatus.Faulted) || _t.Status.Equals(TaskStatus.Canceled))
                    _taskSource.SetResult(false);
            });

            return _taskSource.Task;
        }
        # endregion

        # region Subscribe
        protected override object HandleSubscribeMessage(string subscribeQueue, bool autoAck = false, bool durable = true)
        {
            object _uniqueID = null;
            try
            {
                SessionMode _ackMode = autoAck ? SessionMode.AutoAcknowledge : SessionMode.ExplicitClientAcknowledge;

                // connection
                var _connection = ConnectionFactory.CreateConnection();
                // start
                _connection.Start();

                var _session = _connection.CreateSession(false, _ackMode);
                _uniqueID = _session.SessID;

                // add to the current sessions
                CurrentConsumerSessions.TryAdd(_session.SessID, _session);

                // consumer
                var _destination = _session.CreateTopic(subscribeQueue);
                //var _destination = _lookup.LookupDestination(subscribeQueue);  // handle topic
                var _consumer = _session.CreateConsumer(_destination);
                
                // message listener
                _consumer.MessageHandler += (_o, _m) => { 
                    
                    this.IncomingMessageBuffer.Add(Tuple.Create(_m.Message.Destination.ToString(),  _m.Message.ToString()));
                    Console.WriteLine(_m.Message);
                    _m.Message.Acknowledge();
                };
            }
            catch (Exception ex)
            {

            }

            return _uniqueID;
        }

        protected override void HandleUnsubscribe(object providerHandle)
        {
            long _sessionID = -1;
            try
            {
                _sessionID = (long)providerHandle;
                Session _session;

                if (CurrentConsumerSessions.TryRemove(_sessionID, out _session))
                {
                    _session.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }
        # endregion

        # region Dispose
        protected override void HandleDispose()
        {
        }
        # endregion
    }

    internal class TibcoEMSLookup
    {
        Hashtable _environment = new Hashtable();
        ILookupContext _context = null;

        internal TibcoEMSLookup(IDictionary<string, string> configProperties)
        {
            _environment.Add(LookupContext.PROVIDER_URL, configProperties["server"]);
        }

        internal TibcoEMSLookup(XElement configElement)
        {
            // to do - initialize the environment
        }

        void InitCtx() 
        {
            if (_context == null)
            {
                var _factory = new LookupContextFactory();
                _context = _factory.CreateContext(LookupContextFactory.TIBJMS_NAMING_CONTEXT, _environment);
            }
        }

        internal ConnectionFactory LookupConnectionFactory(string _factoryName)
        {
            if (_context == null)
                InitCtx();

            return _context.Lookup(_factoryName) as ConnectionFactory;
        }

        internal Destination LookupDestination(string _destinationName)
        {
            if (_context == null)
                InitCtx();

            return _context.Lookup(_destinationName) as Destination;
        }

        internal bool IsQueue(string _destinationName)
        {
            if (_context == null)
                InitCtx();

            return _context.Lookup(_destinationName) is TIBCO.EMS.Queue;
        }

        internal bool IsTopic(string _destinationName)
        {
            if (_context == null)
                InitCtx();

            return _context.Lookup(_destinationName) is TIBCO.EMS.Topic;
        }

    }
}
