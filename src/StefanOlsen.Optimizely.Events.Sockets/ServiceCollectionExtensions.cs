using Microsoft.Extensions.DependencyInjection;

namespace StefanOlsen.Optimizely.Events.Sockets
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddUdpMulticastEventProvider(this IServiceCollection services)
        {
            services.AddEventProvider<UdpMulticastEventProvider>();

            return services;
        }

        public static IServiceCollection AddUdpUnicastEventProvider(this IServiceCollection services)
        {
            services.AddEventProvider<UdpUnicastEventProvider>();

            return services;
        }
    }
}
