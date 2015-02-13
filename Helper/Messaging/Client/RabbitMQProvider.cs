using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Helper.Messaging.Client
{
    public class RabbitMQProvider : AbsMessagingProvider
    {
        # region Private Properties 
        ConnectionFactory ConnectionFactory { get; set; }
        IConnection PublishConnection { get; set; }
        IConnection SubscribeConnection { get; set; }
        # endregion

        # region Constructor
        public RabbitMQProvider(IDictionary<string, string> configProperties)
        {
            ConnectionFactory = new ConnectionFactory
            {
                Endpoint = new AmqpTcpEndpoint(new Uri(configProperties["endpoint"])),
                UserName = configProperties["user"],
                Password = configProperties["pwd"]
            };

            PublishConnection = ConnectionFactory.CreateConnection();
            SubscribeConnection = ConnectionFactory.CreateConnection();

        }
        public RabbitMQProvider(XElement configElement)
        {
            // TODO implement the XElement config
        }
        # endregion

        # region Init
        protected override void InitProvider()
        {
            // TODO
        }
        # endregion

        # region Publish
        protected override Task<bool> HandlePublishMessage(string publishLocation, string replyToLocation, IEnumerable<string> message, string clientID = null, bool autoAck = false, bool durable = true, int priority = 0, long timeToLive = 0)
        {
            var _taskSource = new TaskCompletionSource<bool>();

            try
            {
                // create the model
                var _model = PublishConnection.CreateModel();
                _model.ExchangeDeclare(publishLocation, ExchangeType.Direct);

                // attach some event handlers
                _model.BasicAcks += (_m, _a) =>
                {
                    _taskSource.TrySetResult(true);
                };
                _model.BasicReturn += (_m, _a) =>
                {
                    _taskSource.TrySetException(new Exception(string.Format("No Route: ReplyCode = [{0}], ReplyText = [{1}], RoutingKey = [{2}]", _a.ReplyCode, _a.ReplyText, _a.RoutingKey)));
                };

                // publish
                var _basicProperties = _model.CreateBasicProperties();
                _basicProperties.ReplyTo = publishLocation ?? string.Empty;
                _basicProperties.Timestamp = new AmqpTimestamp();

                _model.BasicPublish(publishLocation, null, _basicProperties, Encoding.UTF8.GetBytes(message.First())); // this is a bug, this should be fixed
            }
            catch (Exception ex)
            {
                _taskSource.SetException(ex);
            }

            return _taskSource.Task;
        }
        # endregion

        # region Subscribe
        protected override object HandleSubscribeMessage(string subscribeQueue, string clientID = null, bool autoAck = false, int depth = 1)
        {
            throw new NotImplementedException();
        }

        protected override void HandleUnsubscribe(object providerHandle)
        {
            throw new NotImplementedException();
        }
        # endregion

        # region Lookup
        protected override bool ExistsDestination(string destination, bool createTemp = false)
        {
            throw new NotImplementedException();
        }

        protected override bool IsQueue(string queueName)
        {
            throw new NotImplementedException();
        }

        protected override bool IsTopic(string topicName)
        {
            throw new NotImplementedException();
        }

        protected override string ProviderDestinationName(string destination)
        {
            throw new NotImplementedException();
        }
        # endregion

        # region Dispose
        protected override void HandleDispose()
        {
            throw new NotImplementedException();
        }
        # endregion

    }
}
