using EPiServer.ServiceLocation;

namespace StefanOlsen.Optimizely.Events.Redis
{
    [Options(ConfigurationSection = "Cms")]
    public class RedisEventProviderOptions
    {
        public string ConnectionString { get; set; }
        public string PubSubChannel { get; set; }
    }
}
