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
        private static string CLIENT_ID_NAME = "CLIENT_ID";
        private string _connectionFactoryName;
        private IDictionary<string, string> _configProperties;
        private TibcoEMSLookup _lookup;
        ConnectionFactory ConnectionFactory { get; set; }
        ConcurrentDictionary<long, Session> CurrentConsumerSessions = new ConcurrentDictionary<long, Session>();
        # endregion

        # region Constructor
        public TibcoEMSProvider(IDictionary<string, string> configProperties)
        {
            _configProperties = configProperties;
            _connectionFactoryName = configProperties["connectionfactory"];
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
        }

        protected Connection InitConnection()
        {
            string _sslidentity;
            if (_configProperties.TryGetValue("sslidentity", out _sslidentity))
            {
                ConnectionFactory.SetTargetHostName(_configProperties["ssltargethostname"]);
                // create connection
                var _fileStoreInfo = ConnectionFactory.GetCertificateStore() as EMSSSLFileStoreInfo;
                _fileStoreInfo.SetSSLClientIdentity(_configProperties["sslidentity"]);
                _fileStoreInfo.SetSSLPassword(_configProperties["sslpassword"].ToCharArray());
            }

            return ConnectionFactory.CreateConnection();
        }
        # endregion

        # region Publish
        protected override async Task<bool> HandlePublishMessage(string publishLocation, string replyToLocation, IEnumerable<string> messages, string clientID = null, bool autoAck = true, bool durable = false, int priority = 4, long timeToLive = 0)
        {
            SessionMode _ackMode = autoAck ? SessionMode.AutoAcknowledge : SessionMode.ExplicitClientAcknowledge;
            MessageDeliveryMode _deliveryMode = MessageDeliveryMode.ReliableDelivery;
            int _priority = 4; // lowest is 0, highest is 9. 0-4 is normal priority, 5-9 expedited priority

            Task<bool> _task = Task<bool>.Factory.StartNew(() =>
            {
                try
                {
                    // connection
                    var _connection = InitConnection();
                    _connection.Start();

                    // non-transacted session
                    var _session = _connection.CreateSession(false, _ackMode);
                    //var _destination = _lookup.IsQueue(publishLocation) ? _session.CreateQueue(publishLocation) as Destination : _session.CreateTopic(publishLocation) as Destination;
                    var _destination = _lookup.LookupDestination(publishLocation);
                    var _replyToDestination = string.IsNullOrWhiteSpace(replyToLocation) ? null : _lookup.LookupDestination(replyToLocation);

                    // create the message producer -- TODO set some of producer properties
                    var _producer = _session.CreateProducer(null);
                    //_producer.MsgDeliveryMode = _deliveryMode;
                    //_producer.Priority = _priority;

                    // message
                    messages.AsParallel().FirstOrDefault(_x => 
                        {
                            var _m = _session.CreateTextMessage(_x);
                            if (_replyToDestination != null)
                                _m.ReplyTo = _replyToDestination;
                            if (!string.IsNullOrWhiteSpace(clientID))
                                _m.SetStringProperty(CLIENT_ID_NAME, clientID);

                            _producer.Send(_destination, _m, _deliveryMode, priority, timeToLive);

                            return false; 
                        });

                    // cleanup
                    //_producer.Close();
                    //_session.Close();
                    _connection.Close();

                    // return 
                    return true;
                }
                catch (Exception ex)
                {
                    //_taskSource.TrySetException(ex);
                    return false;
                }
            }).ContinueWith(_t =>
            {
                if (_t.Status.Equals(TaskStatus.Faulted) || _t.Status.Equals(TaskStatus.Canceled))
                    //_taskSource.SetResult(false);
                    return false;
                else
                    return _t.Result;
            });

            return await _task;
        }
        # endregion

        # region Subscribe
        protected override object HandleSubscribeMessage(string subscribeLocation, string clientID = null, bool autoAck = true, int depth = 1)
        {
            object _uniqueID = null;
            try
            {
                SessionMode _ackMode = autoAck ? SessionMode.AutoAcknowledge : SessionMode.ExplicitClientAcknowledge;

                // connection
                var _connection = InitConnection();
                // start
                _connection.Start();

                var _session = _connection.CreateSession(false, _ackMode);
                _uniqueID = _session.SessID;

                // add to the current sessions
                CurrentConsumerSessions.TryAdd(_session.SessID, _session);

                // consumer
                var _destination = _lookup.LookupDestination(subscribeLocation);
                var _consumer = string.IsNullOrWhiteSpace(clientID) ? _session.CreateConsumer(_destination) : _session.CreateConsumer(_destination, string.Format("{0}='{1}'", CLIENT_ID_NAME, clientID));
                
                // message listener
                _consumer.MessageHandler += (_o, _m) => {
                    this.IncomingMessageBuffer.Add(Tuple.Create(_m.Message.Destination.ToString(),  _m.Message.ToString()));
                    //Console.WriteLine(_m.Message);
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

        # region Lookup
        protected override bool ExistsDestination(string destination, bool createTemp = false)
        {
            return _lookup.LookupDestination(destination) != null;
        }

        protected override bool IsQueue(string queueName)
        {
            return _lookup.IsQueue(queueName);
        }

        protected override bool IsTopic(string topicName)
        {
            return _lookup.IsTopic(topicName);
        }

        protected override string ProviderDestinationName(string destination)
        {
            return _lookup.LookupDestination(destination).ToString();
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

            string _sslidentity;
            if (configProperties.TryGetValue("sslidentity", out _sslidentity))
            {
                // using SSL
                EMSSSLFileStoreInfo _info = new EMSSSLFileStoreInfo();
                _info.SetSSLClientIdentity(_sslidentity);
                _info.SetSSLPassword(configProperties["sslpassword"].ToCharArray());

                // enter into the environment
                _environment.Add(LookupContext.SSL_STORE_INFO, _info);
                _environment.Add(LookupContext.SSL_STORE_TYPE, EMSSSLStoreType.EMSSSL_STORE_TYPE_FILE);
                _environment.Add(LookupContext.SECURITY_PROTOCOL, "ssl");
                _environment.Add(LookupContext.SSL_TARGET_HOST_NAME, configProperties["ssltargethostname"]);
            }
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
