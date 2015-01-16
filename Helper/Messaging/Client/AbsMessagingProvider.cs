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
        public Task<bool> PublishMessage(string publishQueue, string replyToQueue, string message)
        {
            // validate the publish queue

            // validate the reply queue

            var _task = HandlePublishMessage(publishQueue, replyToQueue, message);

            return _task;
        }
        # endregion

        # region Subscribe methods
        public SubscriptionHandle SubscribeMessage(string subscribeQueue, CallbackDelegate callbackMethod)
        {
            // validate the subscribe queue

            // create the Subscription Handle
            var _handle = new SubscriptionHandle();
            _handle.SubscribedToQueue = subscribeQueue;
            
            // put the callback in a dictionary
            CallbackHandlers.AddOrUpdate(subscribeQueue, callbackMethod, (_, __) => callbackMethod);

            // create the subscription 
            _handle.Handle = HandleSubscribeMessage(subscribeQueue);

            return _handle;
        }

        public SubscriptionHandle SubscribeMessage<T>(string subscribeQueue, CallbackDelegate callbackMethod)
        {
            throw new NotImplementedException();
        }

        public SubscriptionHandle SubscribeMessage<T>(string subscribeQueue, CallbackDelegate callbackMethod, Predicate<T> messageSelector)
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

        protected abstract Task<bool> HandlePublishMessage(string publishQueue, string replyToQueue, string message, bool autoAck = false);
        protected abstract object HandleSubscribeMessage(string subscribeQueue, bool autoAck = false, bool durable = true);
        protected abstract void HandleUnsubscribe(object providerHandle);
        protected abstract void HandleDispose();
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
