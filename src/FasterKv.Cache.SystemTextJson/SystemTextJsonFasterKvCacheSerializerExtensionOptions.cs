using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FasterKv.Cache.SystemTextJson;

public class SystemTextJsonFasterKvCacheSerializerExtensionOptions : IFasterKvCacheExtensionOptions
{
    public void AddServices(IServiceCollection services, string name)
    {
        services.ArgumentNotNull(nameof(services));
        name.ArgumentNotNullOrEmpty(nameof(name));

        services.AddSingleton<IFasterKvCacheSerializer>(_ => new SystemTextJsonFasterKvCacheSerializer{Name = name});
    }
}