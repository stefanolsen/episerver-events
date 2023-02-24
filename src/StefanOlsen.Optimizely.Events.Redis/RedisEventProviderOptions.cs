using EPiServer.ServiceLocation;

namespace StefanOlsen.Optimizely.Events.Redis
{
    [Options(ConfigurationSection = "Cms")]
    public class RedisEventProviderOptions
    {
        public string ConnectionString { get; init; }
        public string PubSubChannel { get; init; }
    }
}
