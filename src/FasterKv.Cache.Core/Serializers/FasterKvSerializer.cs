using System.IO;
using FASTER.core;
using MessagePack;

namespace FasterKv.Cache.Core.Serializers;

public class FasterKvSerializer : IObjectSerializer<ValueWrapper>
{
    private Stream? _read;
    private Stream? _write;
    private readonly IFasterKvCacheSerializer _serializer;

    public FasterKvSerializer(IFasterKvCacheSerializer serializer)
    {
        _serializer = serializer.ArgumentNotNull();
    }

    public void BeginSerialize(Stream stream)
    {
        _write = stream;
    }

    public void Serialize(ref ValueWrapper obj)
    {
        obj.DataBytes = _serializer.Serialize(obj.Data);
        MessagePackSerializer.Serialize(_write, obj);
    }

    public void BeginDeserialize(Stream stream)
    {
        _read = stream;
    }

    public void Deserialize(out ValueWrapper obj)
    {
        obj = MessagePackSerializer.Deserialize<ValueWrapper>(_read);
    }

    public void EndSerialize()
    {
    }

    public void EndDeserialize()
    {
    }
}