using System.Buffers;
using FASTER.core;
using FasterKv.Cache.Core.Abstractions;

namespace FasterKv.Cache.Core.Serializers;

internal sealed class FasterKvSerializer<TValue> : BinaryObjectSerializer<ValueWrapper<TValue>>
{
    private readonly ISystemClock _systemClock;
    private readonly IFasterKvCacheSerializer _serializer;

    public FasterKvSerializer(IFasterKvCacheSerializer serializer, ISystemClock systemClock)
    {
        _serializer = serializer.ArgumentNotNull();
        _systemClock = systemClock.ArgumentNotNull();
    }

    public override void Deserialize(out ValueWrapper<TValue> obj)
    {
        obj = new ValueWrapper<TValue>();
        var flags = (FasterKvSerializerFlags)reader.ReadByte();
        if ((flags & FasterKvSerializerFlags.HasExpiryTime) == FasterKvSerializerFlags.HasExpiryTime)
        {
            obj.ExpiryTime = reader.ReadInt64();
        }

        if ((flags & FasterKvSerializerFlags.HasBody) == FasterKvSerializerFlags.HasBody)
        {
            var dataLength = reader.ReadInt32();
            if (obj.HasExpired(_systemClock.NowUnixTimestamp()))
            {
                reader.BaseStream.Position += dataLength;
            }
            else
            {
                var buffer = ArrayPool<byte>.Shared.Rent(dataLength);
                try
                {
                    _ = reader.Read(buffer, 0, dataLength);
                    obj.Data = _serializer.Deserialize<TValue>(buffer, dataLength);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
        
    }

    public override void Serialize(ref ValueWrapper<TValue> obj)
    {
        var flags = obj.GetFlags(_systemClock.NowUnixTimestamp());
        writer.Write((byte)flags);
        if ((flags & FasterKvSerializerFlags.HasExpiryTime) == FasterKvSerializerFlags.HasExpiryTime)
        {
            writer.Write(obj.ExpiryTime!.Value);
        }

        if ((flags & FasterKvSerializerFlags.HasBody) == FasterKvSerializerFlags.HasBody)
        {
            var beforePos = writer.BaseStream.Position;
            var dataPos = writer.BaseStream.Position = writer.BaseStream.Position += sizeof(int);
            _serializer.Serialize(writer.BaseStream, obj.Data);
            var afterPos = writer.BaseStream.Position;

            var length = (int)(afterPos - dataPos);
            writer.BaseStream.Position = beforePos;

            writer.Write(length);
            writer.BaseStream.Position = afterPos;
        }
    }
}