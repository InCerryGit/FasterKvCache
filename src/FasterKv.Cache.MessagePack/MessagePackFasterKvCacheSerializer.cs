using System.Buffers;
using System.IO;
using FasterKv.Cache.Core;
using MessagePack;

namespace FasterKv.Cache.MessagePack;

public sealed class MessagePackFasterKvCacheSerializer : IFasterKvCacheSerializer
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

    public void Serialize<TValue>(Stream stream, TValue data)
    {
        MessagePackSerializer.Serialize(stream, data);
    }

    public TValue? Deserialize<TValue>(byte[] serializerData, int length)
    {
        return MessagePackSerializer.Deserialize<TValue>(new ReadOnlySequence<byte>(serializerData, 0, length));
    }
}