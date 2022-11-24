using System;
using System.IO;
using FasterKv.Cache.Core;

namespace FasterKv.Cache.SystemTextJson;

public sealed class SystemTextJsonFasterKvCacheSerializer : IFasterKvCacheSerializer
{
    public string Name { get; set; } = "SystemTextJson";

    public void Serialize<TValue>(Stream stream, TValue data)
    {
        System.Text.Json.JsonSerializer.Serialize(stream, data);
    }

    public TValue? Deserialize<TValue>(byte[] serializerData, int length)
    {
        return System.Text.Json.JsonSerializer.Deserialize<TValue>(new Span<byte>(serializerData,0, length));
    }
}