using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Messaging.Client
{
    public abstract class AbsMessagingProvider : IMessagingProvider, IDisposable
    {
        # region Public Properties
        public const long DEFAULT_TIME_TO_LIVE = 60000L;
        # endregion

        # region Private Properties
        ConcurrentDictionary<string, CallbackDelegate> CallbackHandlers = new ConcurrentDictionary<string, CallbackDelegate>();
        protected BlockingCollection<Tuple<string, string>> IncomingMessageBuffer = new BlockingCollection<Tuple<string, string>>();
        # endregion

        # region Init
        public void Init()
        {
            // start a task that will essentially pump the incoming messages to the correct client location
            Task.Factory.StartNew(() => {
                try
                {
                    foreach (var _m in IncomingMessageBuffer.GetConsumingEnumerable())
                    {
                        Task.Factory.StartNew(() => { 
                            try
                            {
                                // get the correct callback handler
                                CallbackDelegate _handler;
                                if (CallbackHandlers.TryGetValue(_m.Item1, out _handler))
                                {
                                    // invoke the handler to pass in the message
                                    _handler.Invoke(_m.Item1, _m.Item2);
                                }
                            }
                            catch { }
                        });
                    }
                }
                catch { }
            });

            // hook into the provider init
            InitProvider();
        }
        # endregion

        # region Publish methods
        public Task<bool> Publish<T>(string publishLocation, string replyToLocation, IEnumerable<T> payload, string clientID = null, bool autoAck = true, bool durable = false)
        {
            var _autoAckMode = autoAck;
            var _durableMode = durable;
            var _priority = 4;
            var _timeToLive = DEFAULT_TIME_TO_LIVE;

            // validate the publish queue
            if (string.IsNullOrWhiteSpace(publishLocation))
                throw new Exception("Publish location is not specified");

            if (!ExistsDestination(publishLocation))
                throw new Exception(string.Format("Publish location [{0}] does not exist", publishLocation));

            if (IsTopic(publishLocation))
            {
                // acks should be always automatic when publishing to a topic
                _autoAckMode = true;

                // messages should not be durable when publishing to Topic
                _durableMode = false;
            }

            // validate the reply queue
            if (!string.IsNullOrWhiteSpace(replyToLocation) && !ExistsDestination(replyToLocation))
                throw new Exception(string.Format("Reply location [{0}] does not exist", replyToLocation));

            var _task = HandlePublishMessage(publishLocation, replyToLocation, payload.Select(_x => _x.ToString()), clientID, _autoAckMode, _durableMode, _priority, _timeToLive);

            return _task;
        }
        # endregion

        # region Subscribe methods
        public SubscriptionHandle Subscribe(string subscribeLocation, CallbackDelegate callbackMethod, string clientID = null, bool autoAck = true, int depth = 1)
        {
            var _autoAckMode = autoAck;

            // validate the subscribe location
            if (string.IsNullOrWhiteSpace(subscribeLocation))
                throw new Exception("Subscribe location is not specified");

            if (!ExistsDestination(subscribeLocation))
                throw new Exception(string.Format("Subscribe location [{0}] does not exist", subscribeLocation));

            if (IsTopic(subscribeLocation))
                _autoAckMode = true;

            // create the Subscription Handle
            var _handle = new SubscriptionHandle();
            _handle.SubscribedToQueue = subscribeLocation;
            
            // put the callback in a dictionary
            CallbackHandlers.AddOrUpdate(ProviderDestinationName(subscribeLocation), callbackMethod, (_, __) => callbackMethod);

            // create the subscription 
            _handle.Handle = HandleSubscribeMessage(subscribeLocation, clientID,  _autoAckMode, depth);

            return _handle;
        }

        public SubscriptionHandle Subscribe<T>(string subscribeLocation, CallbackDelegate callbackMethod)
        {
            throw new NotImplementedException();
        }

        public SubscriptionHandle Subscribe<T>(string subscribeLocation, CallbackDelegate callbackMethod, Predicate<T> messageSelector)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(SubscriptionHandle handle)
        {
            HandleUnsubscribe(handle.Handle);
        }
        # endregion

        # region Abstract Methods implementation in providers
        protected abstract void InitProvider();

        protected abstract Task<bool> HandlePublishMessage(string publishQueue, string replyToQueue, IEnumerable<string> messages, string clientID = null, bool autoAck = true, bool durable = false, int priority = 4, long timeToLive = 0);
        protected abstract object HandleSubscribeMessage(string subscribeQueue, string clientID = null, bool autoAck = true, int depth = 1);
        protected abstract void HandleUnsubscribe(object providerHandle);
        protected abstract void HandleDispose();
        protected abstract bool ExistsDestination(string destination, bool createTemp = false);
        protected abstract bool IsQueue(string queueName);
        protected abstract bool IsTopic(string topicName);
        protected abstract string ProviderDestinationName(string destination);
        # endregion

        # region Disposable
        public void Dispose()
        {
            // handle other disposal tasks

            // call the provider dispose
            HandleDispose();
        }
        # endregion
}
}
