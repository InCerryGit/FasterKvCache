using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Configurations;

namespace FasterKv.Cache.SystemTextJson;

public static class FasterKvCacheOptionsExtensions
{
    /// <summary>
    /// Adds the FasterKv Cache System.Text.Json Serializer(specify the config via hard code).
    /// </summary>
    public static FasterKvCacheOptions UseSystemTextJsonSerializer(
        this FasterKvCacheOptions options
    )
    {
        options.ArgumentNotNull(nameof(options));

        options.RegisterExtension(new SystemTextJsonFasterKvCacheSerializerExtensionOptions());
        return options;
    }
}