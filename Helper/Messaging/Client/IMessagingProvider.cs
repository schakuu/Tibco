using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Helper.Messaging;
namespace Helper.Messaging.Client
{
    public interface IMessagingProvider
    {
        // init
        void Init();

        // publish
        Task<bool> Publish<T>(string publishLocation, string replyLocation, IEnumerable<T> payload, string clientID = null, bool autoAck = true, bool durable = false);

        // subscribe
        SubscriptionHandle Subscribe(string subscribeLocation, CallbackDelegate callbackMethod, string clientID = null, bool autoAck = true, int depth = 1);
        SubscriptionHandle Subscribe<T>(string subscribeLocation, CallbackDelegate<T> callbackMethod);
        SubscriptionHandle Subscribe<T>(string subscribeLocation, CallbackDelegate<T> callbackMethod, Predicate<T> messageSelector);
        void Unsubscribe(SubscriptionHandle handle);
    }
}
