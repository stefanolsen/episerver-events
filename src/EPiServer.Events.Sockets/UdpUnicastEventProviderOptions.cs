using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;

namespace EPiServer.Events.Sockets
{
    [Options(ConfigurationSection = "Cms")]
    public class UdpUnicastEventProviderOptions
    {
        public string BindHost { get; init; } = "0.0.0.0";
        public int Port { get; init; } = 6000;

        public ICollection<UdpEndpoint> Endpoints { get; init; } = Array.Empty<UdpEndpoint>();
    }

    public class UdpEndpoint
    {
        public string Host { get; init; }
        public int Port { get; init; }
    }
}
