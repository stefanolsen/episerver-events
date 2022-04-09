using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EPiServer.Events.Providers;
using EPiServer.Logging;

namespace EPiServer.Events.Sockets
{
    public sealed class UdpMulticastEventProvider : EventProvider, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(UdpMulticastEventProvider));

        private readonly ActionBlock<EventMessage> _sender;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly UdpMulticastEventProviderOptions _options;
        private readonly IPAddress _ipAddress;
        private readonly DataContractSerializer _serializer;
        private readonly UdpClient _udpClient;

        public UdpMulticastEventProvider(EventsServiceKnownTypesLookup knownTypesLookup, UdpMulticastEventProviderOptions options)
        {
            _options = options;
            _ipAddress = IPAddress.Parse(_options.Address);

            _serializer = new DataContractSerializer(typeof(EventMessage), knownTypesLookup.KnownTypes);
            _udpClient = new UdpClient(_options.Port, _ipAddress.AddressFamily);

            _sender = new ActionBlock<EventMessage>(
                SendMessageInternal,
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });
        }

        public override Task InitializeAsync()
        {
            // Ensure that we will not receive our own messages.
            _udpClient.MulticastLoopback = false;
            
            _udpClient.JoinMulticastGroup(_ipAddress);

            Task.Factory.StartNew(BeginReceiveAsync, _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        public override void SendMessage(EventMessage message)
        {
            throw new NotImplementedException();
        }

        public override async Task SendMessageAsync(EventMessage message, CancellationToken cancellationToken)
        {
            await _sender.SendAsync(message, cancellationToken);
        }

        public override void Uninitialize()
        {
            // Stop the send and receive loops gracefully.
            _cancellationTokenSource.Cancel();

            _udpClient.DropMulticastGroup(_ipAddress);
            _udpClient.Close();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
            _udpClient?.Dispose();
        }

        private async Task BeginReceiveAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var result = await _udpClient.ReceiveAsync();

                EventMessage message;
                try
                {
                    await using var stream = new MemoryStream(result.Buffer);
                    message = (EventMessage)_serializer.ReadObject(stream);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed deserializing event message.", ex);
                    continue;
                }

                OnMessageReceived(new EventMessageEventArgs(message));
            }
        }

        private async Task SendMessageInternal(EventMessage message)
        {
            await using var memoryStream = new MemoryStream();
            _serializer.WriteObject(memoryStream, message);

            await _udpClient.SendAsync(memoryStream.ToArray(), (int)memoryStream.Length, new IPEndPoint(_ipAddress, _options.Port));
        }
    }
}
