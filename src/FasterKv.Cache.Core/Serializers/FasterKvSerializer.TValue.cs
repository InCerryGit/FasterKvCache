using System;
using System.IO;
using FASTER.core;
using MessagePack;

namespace FasterKv.Cache.Core.Serializers;

public class FasterKvSerializer<TValue> : IObjectSerializer<ValueWrapper<TValue>>
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

    public void Serialize(ref ValueWrapper<TValue> obj)
    {
        var pack = new DataPack
        {
            ExpiryTime = obj.ExpiryTime,
            SerializerData = _serializer.Serialize(obj.Data)
        };
        MessagePackSerializer.Serialize(_write, pack);
    }

    public void BeginDeserialize(Stream stream)
    {
        _read = stream;
    }

    public void Deserialize(out ValueWrapper<TValue> obj)
    {
        var pack = MessagePackSerializer.Deserialize<DataPack>(_read);
        obj = new ValueWrapper<TValue>(_serializer.Deserialize<TValue>(pack.SerializerData), pack.ExpiryTime);
    }

    public void EndSerialize()
    {
    }

    public void EndDeserialize()
    {
    }
    
    [MessagePackObject]
    public struct DataPack
    {    
        /// <summary>
        /// Expiry Time
        /// </summary>
        [Key(0)]
        public DateTimeOffset? ExpiryTime { get; set; }

        [Key(1)]
        public byte[] SerializerData { get; set; }
    }
}