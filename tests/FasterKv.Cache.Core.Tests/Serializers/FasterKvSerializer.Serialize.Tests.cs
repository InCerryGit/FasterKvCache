using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Serializers;
using Moq;

namespace FasterKv.Cache.Core.Tests.Serializers;

public class FasterKvSerializerSerializeTests
{
    [Fact]
    public void Expired_Value_Should_Only_Serialize_ExpiryTime()
    {
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        using var ms = new MemoryStream();
        ser.BeginSerialize(ms);

        var wrapper = new ValueWrapper("", 80);
        ser.Serialize(ref wrapper);
        
        // | flag | timestamp |
        // |  1B  |    8B     |
        Assert.Equal(1 + 8, ms.Position);
    }

    [Fact]
    public void NotExpiry_Value_Should_Serialize_All_Member()
    {
        int serializeLength = 10;
        
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();
        mockKvCache.Setup(i => i.Serialize(It.IsAny<Stream>(), It.IsAny<object>())).Callback<Stream, object>(
            (stream, data) =>
            {
                stream.Position += serializeLength;
            });

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        using var ms = new MemoryStream();
        ser.BeginSerialize(ms);

        var wrapper = new ValueWrapper("", 110);
        ser.Serialize(ref wrapper);
        
        // | flag | timestamp | data length | serialize length|
        // |  1B  |    8B     |     4B      |      8B         |
        Assert.Equal(1 + 8 + 4 + serializeLength, ms.Position);
    }

    [Fact]
    public void Not_Value_And_Not_ExpiryTime_Should_Only_Serialize_Flag()
    {
        int serializeLength = 10;
        
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();
        mockKvCache.Setup(i => i.Serialize(It.IsAny<Stream>(), It.IsAny<object>())).Callback<Stream, object>(
            (stream, data) =>
            {
                stream.Position += serializeLength;
            });

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        using var ms = new MemoryStream();
        ser.BeginSerialize(ms);

        var wrapper = new ValueWrapper(null, null);
        ser.Serialize(ref wrapper);
        
        // | flag |
        // |  1B  |
        Assert.Equal(1 , ms.Position);
    }

    [Fact]
    public void Not_ExpiryTime_Should_Serialize_Flag_DataLength_Value()
    {
        int serializeLength = 10;
        
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();
        mockKvCache.Setup(i => i.Serialize(It.IsAny<Stream>(), It.IsAny<object>())).Callback<Stream, object>(
            (stream, data) =>
            {
                stream.Position += serializeLength;
            });

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        using var ms = new MemoryStream();
        ser.BeginSerialize(ms);

        var wrapper = new ValueWrapper("", null);
        ser.Serialize(ref wrapper);
        
        // | flag | data length | serialize length |
        // |  1B  |    4B       |      10B         |
        Assert.Equal(1 + 4 + 10, ms.Position);
    }
}