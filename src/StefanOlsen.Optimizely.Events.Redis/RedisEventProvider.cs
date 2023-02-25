using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using EPiServer.Events;
using EPiServer.Events.Providers;
using EPiServer.Events.Providers.Internal;
using EPiServer.Logging;
using StackExchange.Redis;

namespace StefanOlsen.Optimizely.Events.Redis
{
    public class RedisEventProvider : EventProvider
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(RedisEventProvider));
        private readonly string _applicationName;
        private readonly string _machineName;
        private readonly RedisEventProviderOptions _options;
        private readonly DataContractSerializer _serializer;
        private IConnectionMultiplexer _connectionMultiplexer;
        private ISubscriber _subscriber;

        public RedisEventProvider(
            IEventHostingEnvironment eventHostingEnvironment,
            EventsServiceKnownTypesLookup knownTypesLookup,
            RedisEventProviderOptions options)
        {
            _applicationName = eventHostingEnvironment.ApplicationName;
            _machineName = Environment.MachineName;
            _options = options;

            _serializer = new DataContractSerializer(typeof(EventMessage), knownTypesLookup.KnownTypes);
        }

        public override async Task InitializeAsync()
        {
            _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(_options.ConnectionString);
            _subscriber = _connectionMultiplexer.GetSubscriber();
            
            await _subscriber.SubscribeAsync(_options.PubSubChannel, OnMessageReceived);
        }

        public override void SendMessage(EventMessage message)
        {
            throw new NotImplementedException();
        }

        public override async Task SendMessageAsync(EventMessage message, CancellationToken cancellationToken)
        {
            await using var memoryStream = new MemoryStream();
            _serializer.WriteObject(memoryStream, message);

            await _subscriber.PublishAsync(_options.PubSubChannel, memoryStream.ToArray());
        }

        public override void Uninitialize()
        {
            _subscriber?.Unsubscribe(_options.PubSubChannel);
            _connectionMultiplexer.Dispose();
        }

        private void OnMessageReceived(RedisChannel channel, RedisValue value)
        {
            if (channel.IsNullOrEmpty ||
                value.IsNullOrEmpty)
            {
                return;
            }

            EventMessage message;
            try
            {
                using var stream = new MemoryStream(value);
                message = (EventMessage)_serializer.ReadObject(stream);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed deserializing event message.", ex);
                return;
            }

            // If the message originated from here, ignore it.
            if (message == null ||
                message.ServerName == _machineName &&
                message.ApplicationName == _applicationName)
            {
                Logger.Debug("Received event originated from this server itself.");
                return;
            }

            OnMessageReceived(new EventMessageEventArgs(message));
        }
    }
}