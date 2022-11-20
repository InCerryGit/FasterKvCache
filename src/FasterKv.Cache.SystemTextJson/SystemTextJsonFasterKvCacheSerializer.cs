using FasterKv.Cache.Core;

namespace FasterKv.Cache.SystemTextJson;

public class SystemTextJsonFasterKvCacheSerializer : IFasterKvCacheSerializer
{
    public string Name { get; set; } = "SystemTextJson";
    public byte[] Serialize<TValue>(TValue data)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
    }

    public TValue? Deserialize<TValue>(byte[] serializerData)
    {
        return System.Text.Json.JsonSerializer.Deserialize<TValue>(serializerData);
    }
}