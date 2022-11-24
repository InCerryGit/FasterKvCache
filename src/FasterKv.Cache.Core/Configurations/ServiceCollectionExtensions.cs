using System;
using FasterKv.Cache.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FasterKv.Cache.Core.Configurations;

public static class ServiceCollectionExtensions
{
    const string DefaultFasterKvCacheName = "FasterKvCache";
    const string DefaultFasterKvCacheTValueName = "FasterKvCacheTValue";

    /// <summary>
    /// Adds the FasterKvCache (specify the config via hard code).
    /// </summary>
    public static IServiceCollection AddFasterKvCache(
        this IServiceCollection services,
        Action<FasterKvCacheOptions> setupAction,
        string name = DefaultFasterKvCacheName
    )
    {
        services.ArgumentNotNull(nameof(services));
        setupAction.ArgumentNotNull(nameof(setupAction));

        var option = new FasterKvCacheOptions();
        setupAction(option);
        foreach (var extension in option.Extensions)
        {
            extension.AddServices(services, name);
        }

        services.Configure<FasterKvCacheOptions>(name, x =>
        {
            x.IndexCount = option.IndexCount;
            x.PageSizeBit = option.PageSizeBit;
            x.LogPath = option.LogPath;
            x.MemorySizeBit = option.MemorySizeBit;
            x.ExpiryKeyScanInterval = option.ExpiryKeyScanInterval;
            x.SerializerName = option.SerializerName;
            x.ReadCacheMemorySizeBit = option.ReadCacheMemorySizeBit;
            x.ReadCachePageSizeBit = option.ReadCachePageSizeBit;
            x.CustomStore = option.CustomStore;
        });
        services.TryAddSingleton<ISystemClock, DefaultSystemClock>();
        services.AddSingleton(provider =>
        {
            var optionsMon = provider.GetRequiredService<IOptionsMonitor<FasterKvCacheOptions>>();
            var options = optionsMon.Get(name);
            var factory = provider.GetService<ILoggerFactory>();
            var serializers = provider.GetServices<IFasterKvCacheSerializer>();
            var clock = provider.GetService<ISystemClock>();
            return new FasterKvCache(name, clock!, options, serializers, factory);
        });
        return services;
    }

    /// <summary>
    /// Adds the FasterKvCache (specify the config via hard code).
    /// </summary>
    public static IServiceCollection AddFasterKvCache<TValue>(
        this IServiceCollection services,
        Action<FasterKvCacheOptions> setupAction,
        string name = DefaultFasterKvCacheTValueName
    )
    {
        services.ArgumentNotNull(nameof(services));
        setupAction.ArgumentNotNull(nameof(setupAction));

        var option = new FasterKvCacheOptions();
        setupAction(option);
        foreach (var extension in option.Extensions)
        {
            extension.AddServices(services, name);
        }

        services.Configure<FasterKvCacheOptions>(name, x =>
        {
            x.IndexCount = option.IndexCount;
            x.PageSizeBit = option.PageSizeBit;
            x.LogPath = option.LogPath;
            x.MemorySizeBit = option.MemorySizeBit;
            x.ExpiryKeyScanInterval = option.ExpiryKeyScanInterval;
            x.SerializerName = option.SerializerName;
            x.ReadCacheMemorySizeBit = option.ReadCacheMemorySizeBit;
            x.ReadCachePageSizeBit = option.ReadCachePageSizeBit;
            x.CustomStore = option.CustomStore;
        });
        services.TryAddSingleton<ISystemClock, DefaultSystemClock>();
        services.AddSingleton(provider =>
        {
            var optionsMon = provider.GetRequiredService<IOptionsMonitor<FasterKvCacheOptions>>();
            var options = optionsMon.Get(name);
            var factory = provider.GetService<ILoggerFactory>();
            var serializers = provider.GetServices<IFasterKvCacheSerializer>();
            var clock = provider.GetService<ISystemClock>();
            return new FasterKvCache<TValue>(name, clock!, options, serializers, factory);
        });
        return services;
    }
}