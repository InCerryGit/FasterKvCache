using MessagePack;

namespace FasterKv.Cache.Core.Tests.Serializers;

public class MessagePackTests
{
    [Fact]
    public void Serializer_Null_Should_Success()
    {
        Test obj = null!;
        var bytes = MessagePackSerializer.Serialize(obj);
        var result = MessagePackSerializer.Deserialize<Test>(bytes);
        Assert.Equal(obj, result);
    }
}

[MessagePackObject]
public class Test
{
    [Key(0)]
    public string Value { get; set; } = null!;
}