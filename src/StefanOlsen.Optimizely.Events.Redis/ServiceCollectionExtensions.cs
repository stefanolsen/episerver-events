using Microsoft.Extensions.DependencyInjection;

namespace StefanOlsen.Optimizely.Events.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisEventProvider(this IServiceCollection services)
    {
        services.AddEventProvider<RedisEventProvider>();

        return services;
    }
}