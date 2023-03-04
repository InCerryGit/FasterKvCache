using System.Buffers;
using System.IO;
using FasterKv.Cache.Core;
using MemoryPack;
using MemoryPack.Internal;

namespace FasterKv.Cache.MemoryPack;

public sealed class MemoryPackFasterKvCacheSerializer : IFasterKvCacheSerializer
{
    public string Name { get; set; } = "MemoryPack";
    
    public void Serialize<TValue>(Stream stream, TValue data)
    {
        var writer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            MemoryPackSerializer.Serialize(writer, data);
            var span = writer.GetSpan();
            stream.Write(span[..writer.TotalWritten]);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(writer);
        }
    }

    public TValue? Deserialize<TValue>(byte[] serializerData, int length)
    {
        return MemoryPackSerializer.Deserialize<TValue>(new ReadOnlySequence<byte>(serializerData, 0, length));
    }
}