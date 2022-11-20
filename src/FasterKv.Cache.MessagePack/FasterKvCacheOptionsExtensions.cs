using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Configurations;

namespace FasterKv.Cache.MessagePack;

public static class FasterKvCacheOptionsExtensions
{
    /// <summary>
    /// Adds the FasterKv Cache Message Pack Serializer(specify the config via hard code).
    /// </summary>
    public static FasterKvCacheOptions UseMessagePackSerializer(
        this FasterKvCacheOptions options
    )
    {
        options.ArgumentNotNull(nameof(options));

        options.RegisterExtension(new MessagePackFasterKvCacheSerializerExtensionOptions());
        return options;
    }
}