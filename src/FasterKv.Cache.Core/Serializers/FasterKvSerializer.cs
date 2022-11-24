using System.Buffers;
using FASTER.core;

namespace FasterKv.Cache.Core.Serializers;

internal sealed class FasterKvSerializer : BinaryObjectSerializer<ValueWrapper>
{
    private readonly IFasterKvCacheSerializer _serializer;

    public FasterKvSerializer(IFasterKvCacheSerializer serializer)
    {
        _serializer = serializer.ArgumentNotNull();
    }

    public override void Deserialize(out ValueWrapper obj)
    {
        obj = new ValueWrapper();
        var etNullFlag = reader.ReadByte();
        if (etNullFlag == 1)
        {
            obj.ExpiryTime = reader.ReadInt64();
        }

        obj.DataByteLength = reader.ReadInt32();
        obj.DataBytes = ArrayPool<byte>.Shared.Rent(obj.DataByteLength);
        _ = reader.Read(obj.DataBytes, 0, obj.DataByteLength);
    }

    public override void Serialize(ref ValueWrapper obj)
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