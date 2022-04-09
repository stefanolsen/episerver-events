using EPiServer.ServiceLocation;

namespace EPiServer.Events.Sockets
{
    [Options(ConfigurationSection = "Cms")]
    public class UdpMulticastEventProviderOptions
    {
        public string Address { get; init; } = "239.255.255.19";
        public int Port { get; init; } = 6000;
    }
}
