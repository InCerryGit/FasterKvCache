using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FasterKv.Cache.MessagePack;

public class MessagePackFasterKvCacheSerializerExtensionOptions : IFasterKvCacheExtensionOptions
{
    public void AddServices(IServiceCollection services, string name)
    {
        services.ArgumentNotNull(nameof(services));
        name.ArgumentNotNullOrEmpty(nameof(name));

        services.AddSingleton<IFasterKvCacheSerializer>(_ => new MessagePackFasterKvCacheSerializer {Name = name});
    }
}