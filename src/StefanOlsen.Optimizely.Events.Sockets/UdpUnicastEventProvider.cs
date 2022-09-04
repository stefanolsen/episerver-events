using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using EPiServer.Events;
using EPiServer.Events.Providers;
using EPiServer.Logging;

namespace StefanOlsen.Optimizely.Events.Sockets
{
    public sealed class UdpUnicastEventProvider : EventProvider, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetLogger(typeof(UdpUnicastEventProvider));
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly BroadcastBlock<EventMessage> _sender;
        private readonly DataContractSerializer _serializer;
        private readonly UdpClient _udpClient;

        public UdpUnicastEventProvider(EventsServiceKnownTypesLookup knownTypesLookup, UdpUnicastEventProviderOptions options)
        {
            _serializer = new DataContractSerializer(typeof(EventMessage), knownTypesLookup.KnownTypes);
            _udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(options.BindHost), options.Port));

            _sender = new BroadcastBlock<EventMessage>(msg => msg);
            foreach (var endpoint in options.Endpoints)
            {
                var actionBlock = new ActionBlock<EventMessage>(async msg =>
                {
                    await SendMessageInternal(msg, endpoint.Host, endpoint.Port);
                });

                _sender.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });
            }
        }

        public override Task InitializeAsync()
        {
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
            _cancellationTokenSource.Cancel();
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

        private async Task SendMessageInternal(EventMessage message, string hostname, int port)
        {
            await using var memoryStream = new MemoryStream();
            _serializer.WriteObject(memoryStream, message);

            await _udpClient.SendAsync(memoryStream.ToArray(), (int)memoryStream.Length, hostname, port);
        }
    }
}
