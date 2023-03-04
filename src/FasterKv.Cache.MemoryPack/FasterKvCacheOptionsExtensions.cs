using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Configurations;

namespace FasterKv.Cache.MemoryPack;

public static class FasterKvCacheOptionsExtensions
{
    /// <summary>
    /// Adds the FasterKv Cache Memory Pack Serializer(specify the config via hard code).
    /// </summary>
    public static FasterKvCacheOptions UseMemoryPackSerializer(
        this FasterKvCacheOptions options
    )
    {
        options.ArgumentNotNull();

        options.RegisterExtension(new MemoryPackFasterKvCacheSerializerExtensionOptions());
        return options;
    }
}