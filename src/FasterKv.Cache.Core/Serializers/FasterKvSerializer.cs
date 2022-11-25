using System;
using System.Buffers;
using FASTER.core;
using FasterKv.Cache.Core.Abstractions;

namespace FasterKv.Cache.Core.Serializers;

internal sealed class FasterKvSerializer : BinaryObjectSerializer<ValueWrapper>
{
    private readonly ISystemClock _systemClock;
    private readonly IFasterKvCacheSerializer _serializer;

    public FasterKvSerializer(IFasterKvCacheSerializer serializer, ISystemClock systemClock)
    {
        _systemClock = systemClock.ArgumentNotNull();
        _serializer = serializer.ArgumentNotNull();
    }

    public override void Deserialize(out ValueWrapper obj)
    {
        obj = new ValueWrapper();
        var flags = (FasterKvSerializerFlags)reader.ReadByte();
        if ((flags & FasterKvSerializerFlags.HasExpiryTime) == FasterKvSerializerFlags.HasExpiryTime)
        {
            obj.ExpiryTime = reader.ReadInt64();
        }

        if ((flags & FasterKvSerializerFlags.HasBody) == FasterKvSerializerFlags.HasBody)
        {
            obj.DataByteLength = reader.ReadInt32();
            if (obj.HasExpired(_systemClock.NowUnixTimestamp()))
            {
                reader.BaseStream.Position += obj.DataByteLength;
                obj.DataByteLength = 0;
            }
            else
            {
                obj.DataBytes = ArrayPool<byte>.Shared.Rent(obj.DataByteLength);
                _ = reader.Read(obj.DataBytes, 0, obj.DataByteLength);    
            }
        }
    }

    public override void Serialize(ref ValueWrapper obj)
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

[Flags]
internal enum FasterKvSerializerFlags : byte
{
    None = 0,
    HasExpiryTime = 1 << 0,
    HasBody = 1 << 1
}