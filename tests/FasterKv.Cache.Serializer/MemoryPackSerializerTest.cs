using FasterKv.Cache.Core;
using FasterKv.Cache.MemoryPack;
using MemoryPack;

namespace FasterKv.Cache.Serializer;

public class MemoryPackSerializerTest: BaseSerializerTest<Data>
{
    protected override IFasterKvCacheSerializer GetSerializer()
    {
        return new MemoryPackFasterKvCacheSerializer();
    }

    protected override Data GetSerializerData()
    {
        return new Data(1, 2, "3", "4", "5", 6, 7);
    }
}

[MemoryPackable]
public partial record Data(int P1, int P2, string P3, string P4, string P5, double P6, long P7);