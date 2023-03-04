using FasterKv.Cache.Core;

namespace FasterKv.Cache.Serializer;

public abstract class BaseSerializerTest<T>
{
    protected abstract IFasterKvCacheSerializer GetSerializer();

    protected abstract T GetSerializerData();

    [Fact]
    public void Serializer_Single_Object_Should_Success()
    {
        var data = GetSerializerData();
        var serializer = GetSerializer();

        using var stream = new MemoryStream();
        serializer.Serialize(stream, data);

        stream.Position = 0;
        var newData = serializer.Deserialize<T>(stream.ToArray(), (int) stream.Length);
        
        Assert.Equal(data, newData);
    }
}
