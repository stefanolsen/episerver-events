using EPiServer.ServiceLocation;

namespace StefanOlsen.Optimizely.Events.Sockets
{
    [Options(ConfigurationSection = "Cms")]
    public class UdpMulticastEventProviderOptions
    {
        public string Address { get; set; } = "239.255.255.19";
        public int Port { get; set; } = 6000;
    }
}
