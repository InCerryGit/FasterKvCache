using System.Runtime.CompilerServices;
using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Serializers;
using Moq;

namespace FasterKv.Cache.Core.Tests.Serializers;

public class FasterKvSerializerDeserializeTests
{
    private unsafe Span<byte> ToSpan<T>(ref T value)
    {
        return new Span<byte>(Unsafe.AsPointer(ref value), Unsafe.SizeOf<T>());
    }

    [Fact]
    public void Expired_Value_Should_Only_DeSerialize_ExpiryTime()
    {
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);

        long timeStamp = 1020304;
        // | flag | timestamp |
        // |  1B  |    8B     |
        using var ms = new MemoryStream();
        ms.WriteByte((byte)FasterKvSerializerFlags.HasExpiryTime);
        ms.Write(ToSpan(ref timeStamp));

        ms.Position = 0;
        ser.BeginDeserialize(ms);
        
        ser.Deserialize(out var valueWrapper);
        
        Assert.Equal(0, valueWrapper.DataByteLength);
        Assert.Equal(timeStamp, valueWrapper.ExpiryTime);
        Assert.Null(valueWrapper.DataBytes);
    }

    [Fact]
    public void NotExpiry_Value_Should_Deserialize_All_Member()
    {
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();
        var mockClock = new Mock<ISystemClock>();
        
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        using var ms = new MemoryStream();
        
        // | flag | timestamp | data length | serialize length|
        // |  1B  |    8B     |     4B      |      xxB        |
        ms.WriteByte((byte)(FasterKvSerializerFlags.HasExpiryTime | FasterKvSerializerFlags.HasBody));
        
        long timeStamp = 1020304;
        ms.Write(ToSpan(ref timeStamp));
        
        ReadOnlySpan<byte> data = "hello world"u8;
        int dataLength = data.Length;
        ms.Write(ToSpan(ref dataLength));
        ms.Write(data);
        
        ms.Position = 0;
        ser.BeginDeserialize(ms);
        
        ser.Deserialize(out var wrapper);
        
        Assert.Equal(timeStamp, wrapper.ExpiryTime);
        Assert.Equal(dataLength, wrapper.DataByteLength);
        for (int i = 0; i < wrapper.DataByteLength; i++)
        {
            Assert.Equal(data[i], wrapper.DataBytes![i]);   
        }
    }

    [Fact]
    public void Not_Value_And_Not_ExpiryTime_Should_Only_Deserialize_Flag()
    {
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        // | flag |
        // |  1B  |
        using var ms = new MemoryStream();
        ms.WriteByte((byte)FasterKvSerializerFlags.None);
        ms.Position = 0;
        
        ser.BeginDeserialize(ms);
        ser.Deserialize(out var obj);
        
        Assert.Null(obj.ExpiryTime);
        Assert.Null(obj.DataBytes);
        Assert.Null(obj.Data);
        Assert.Equal(0, obj.DataByteLength);
    }

    [Fact]
    public void Not_ExpiryTime_Should_Deserialize_Flag_DataLength_Value()
    {
        var mockKvCache = new Mock<IFasterKvCacheSerializer>();
        mockKvCache.Setup(i => i.Serialize(It.IsAny<Stream>(), It.IsAny<object>()));

        var mockClock = new Mock<ISystemClock>();
        mockClock.Setup(i => i.NowUnixTimestamp()).Returns(100);
        var ser = new FasterKvSerializer(mockKvCache.Object, mockClock.Object);
        
        // | flag | data length | serialize length |
        // |  1B  |    4B       |      xxB         |
        using var ms = new MemoryStream();
        ser.BeginDeserialize(ms);
        
        ReadOnlySpan<byte> data = "hello world"u8;
        ms.WriteByte((byte)FasterKvSerializerFlags.HasBody);
        var dataLength = data.Length;
        ms.Write(ToSpan(ref dataLength));
        ms.Write(data);

        ms.Position = 0;
        ser.BeginDeserialize(ms);
        
        ser.Deserialize(out var valueWrapper);
        
        Assert.Equal(dataLength, valueWrapper.DataByteLength);
        for (int i = 0; i < valueWrapper.DataByteLength; i++)
        {
            Assert.Equal(data[i], valueWrapper.DataBytes![i]);   
        }
    }
}