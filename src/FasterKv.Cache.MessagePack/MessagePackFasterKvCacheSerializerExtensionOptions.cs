using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FasterKv.Cache.MessagePack;

public sealed class MessagePackFasterKvCacheSerializerExtensionOptions : IFasterKvCacheExtensionOptions
{
    public void AddServices(IServiceCollection services, string name)
    {
        services.ArgumentNotNull();
        name.ArgumentNotNullOrEmpty();

        services.AddSingleton<IFasterKvCacheSerializer>(_ => new MessagePackFasterKvCacheSerializer {Name = name});
    }
}