using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Helper.Messaging;
namespace Helper.Messaging.Client
{
    public interface IMessagingProvider
    {
        // publish
        Task<bool> PublishMessage(string publishQueue, string replyToQueue, string message);

        // subscribe
        SubscriptionHandle SubscribeMessage(string subscribeQueue, CallbackDelegate callbackMethod);
        SubscribeClient SubscribeMessage<T>(string subscribeQueue, CallbackDelegate callbackMethod);
        SubscribeClient SubscribeMessage<T>(string subscribeQueue, CallbackDelegate callbackMethod, Predicate<T> messageSelector);
        void Unsubscribe(SubscriptionHandle handle);
    }
}
