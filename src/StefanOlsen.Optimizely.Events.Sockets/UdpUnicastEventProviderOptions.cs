﻿using System.Collections.Generic;
using EPiServer.ServiceLocation;

namespace StefanOlsen.Optimizely.Events.Sockets
{
    [Options(ConfigurationSection = "Cms")]
    public class UdpUnicastEventProviderOptions
    {
        public string BindHost { get; set; } = "0.0.0.0";
        public int Port { get; set; } = 6000;

        public ICollection<UdpEndpoint> Endpoints { get; set; } = new List<UdpEndpoint>();
    }

    public class UdpEndpoint
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
