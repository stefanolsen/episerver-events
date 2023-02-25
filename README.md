# StefanOlsen.Optimizely.Events.Redis

## Introduction
This repository contains remote event providers that transmit messages using different technologies.

## Configuration (Sockets)
Configure one of the event providers in appsettings.json (or one of its transform files), like this:
```
"EPiServer": {
    "Cms": {
        "EventProvider": {
            // Choose either of the two providers here and remove the other.
            //"Provider": "StefanOlsen.Optimizely.Events.Sockets.UdpMulticastEventProvider, StefanOlsen.Optimizely.Events.Sockets"
            "Provider": "StefanOlsen.Optimizely.Events.Sockets.UdpUnicastEventProvider, StefanOlsen.Optimizely.Events.Sockets"
        },
        // Configure either of the two providers here and remove the other.
        "UdpMulticastEventProvider": {
            "Address": "239.255.255.19",
            "Port": 6000
        }
        "UdpUnicastEventProvider": {
            "BindHost": "0.0.0.0",
            "Port": 6000,
            "Endpoints": [
                { "Host": "server2.local", "Port": 6000 },
                { "Host": "server3.local", "Port": 6000 }
            ]
        }
    }
}
```

Or configure it in Startup.cs, like this:
```
using StefanOlsen.Optimizely.Events.Sockets;

public static class ServiceCollectionExtensions{
    public void ConfigureServices(IServiceCollection services)
    {
        // Copy and edit this block for UDP multicast.
        services.Configure<UdpMulticastEventProviderOptions>(opts =>
        {
            opts.Address = "239.255.255.19";
            opts.Port = 6000;
        });
        services.AddUdpMulticastEventProvider();
        
        // Or this block for UDP unicast.
        services.Configure<UdpUnicastEventProviderOptions>(opts =>
        {
            opts.BindHost = "0.0.0.0";
            opts.Port = 6000;
            opts.Endpoints = new UdpEndpoint[]
            {
                new() { Host = "server2.local", Port = 6000 },
                new() { Host = "server3.local", Port = 6000 }
            };
        });
        services.AddUdpUnicastEventProvider();
    }
}
```

## Configuration (Redis)
Configure one of the event providers in appsettings.json (or one of its transform files), like this:
```
"EPiServer": {
    "Cms": {
        "EventProvider": {
            "Provider": "StefanOlsen.Optimizely.Events.Redis.RedisEventProvider, StefanOlsen.Optimizely.Events.Redis"
        },
        "RedisEventProvider": {
            // Connection string can also contain credentials, cluster-specific settings etc.
            "ConnectionString": "redis-srv.local",
            // Colons can be used to build a hierarchical channel key.
            "PubSubChannel": "optimizely:staging:events"
        }
    }
}
```

Or configure it in Startup.cs, like this:
```
using StefanOlsen.Optimizely.Events.Redis;

public static class ServiceCollectionExtensions{
    public void ConfigureServices(IServiceCollection services)
    {
        // Copy and edit this block for UDP multicast.
        services.Configure<RedisEventProviderOptions>(opts =>
        {
            opts.ConnectionString = "redis-srv.local";
            opts.PubSubChannel = "optimizely:staging:events";
        });
        services.AddRedisEventProvider();
    }
}
```