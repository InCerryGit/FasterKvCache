﻿using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FasterKv.Cache.SystemTextJson;

public sealed class SystemTextJsonFasterKvCacheSerializerExtensionOptions : IFasterKvCacheExtensionOptions
{
    public void AddServices(IServiceCollection services, string name)
    {
        services.ArgumentNotNull();
        name.ArgumentNotNullOrEmpty();

        services.AddSingleton<IFasterKvCacheSerializer>(_ => new SystemTextJsonFasterKvCacheSerializer{Name = name});
    }
}