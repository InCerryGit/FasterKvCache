using FasterKv.Cache.Core;
using MessagePack;

namespace FasterKv.Cache.MessagePack;

public class MessagePackFasterKvCacheSerializer : IFasterKvCacheSerializer
{
    public string Name { get; set; } = "MessagePack";
    public byte[] Serialize<TValue>(TValue data)
    {
        return MessagePackSerializer.Serialize(data);
    }

    public TValue Deserialize<TValue>(byte[] serializerData)
    {
        return MessagePackSerializer.Deserialize<TValue>(serializerData);
    }
}