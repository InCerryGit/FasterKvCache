using System.Buffers;
using FASTER.core;

namespace FasterKv.Cache.Core.Serializers;

internal sealed class FasterKvSerializer<TValue> : BinaryObjectSerializer<ValueWrapper<TValue>>
{
    private readonly IFasterKvCacheSerializer _serializer;

    public FasterKvSerializer(IFasterKvCacheSerializer serializer)
    {
        _serializer = serializer.ArgumentNotNull();
    }

    public override void Deserialize(out ValueWrapper<TValue> obj)
    {
        obj = new ValueWrapper<TValue>();
        var etNullFlag = reader.ReadByte();
        if (etNullFlag == 1)
        {
            obj.ExpiryTime = reader.ReadInt64();
        }

        var dataLength = reader.ReadInt32();
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

    public override void Serialize(ref ValueWrapper<TValue> obj)
    {
        if (obj.ExpiryTime is null)
        {
            // write Expiry Time is null flag
            writer.Write((byte)0);
        }
        else
        {
            writer.Write((byte)1);
            writer.Write(obj.ExpiryTime.Value);
        }

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