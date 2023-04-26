using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;

namespace FasterKv.Cache.Core.Tests.KvStore;

public class FasterKvStoreObjectTestGetOrAdd
{
    private static FasterKvCache CreateKvStore(string guid, ISystemClock? systemClock = null)
    {
        return new FasterKvCache(null!,
            systemClock ?? new DefaultSystemClock(),
            new FasterKvCacheOptions
            {
                IndexCount = 16384,
                MemorySizeBit = 10,
                PageSizeBit = 10,
                ReadCacheMemorySizeBit = 10,
                ReadCachePageSizeBit = 10,
                SerializerName = "MessagePack",
                ExpiryKeyScanInterval = TimeSpan.FromSeconds(1),
                LogPath = $"./unit-test/{guid}"
            },
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MessagePack"
                }
            },
            null);
    }
    
    [Fact]
    public void GetOrAdd_Should_Return_Existing_Value()
    {

        var guid = Guid.NewGuid().ToString("N");
        using var fasterKv = CreateKvStore(guid);
        
        var data = new Data
        {
            One = "one",
            Two = 2
        };
        fasterKv.Set(guid, data);

        var result = fasterKv.GetOrAdd<Data>(guid, _ => new Data
        {
            One = "two",
            Two = 3
        });

        Assert.Equal(data, result);
    }
    
    [Fact]
    public void GetOrAdd_Should_Return_New_Value()
    {
        var guid = Guid.NewGuid().ToString("N");
        using var fasterKv = CreateKvStore(guid);

        var result = fasterKv.GetOrAdd<Data>(guid, (_) => new Data
        {
            One = "two",
            Two = 3
        });

        Assert.Equal("two", result.One);
        Assert.Equal(3, result.Two);
    }
    
    [Fact]
    public void GetOrAdd_Should_Return_NewValue_When_Expired()
    {
        var guid = Guid.NewGuid().ToString("N");
        var mockSystemClock = new MockSystemClock(DateTimeOffset.Now);
        using var fasterKv = CreateKvStore(guid, mockSystemClock);

        var data = new Data
        {
            One = "one",
            Two = 2
        };
        fasterKv.Set(guid, data, TimeSpan.FromSeconds(1));
        
        mockSystemClock.AddSeconds(2);

        var result = fasterKv.GetOrAdd(guid, _ => new Data
        {
            One = "two",
            Two = 3
        });

        Assert.Equal("two", result.One);
        Assert.Equal(3, result.Two);
    }
    
    [Fact]
    public void GetOrAdd_Should_Return_NewValue_When_Expired_And_Refresh()
    {
        var guid = Guid.NewGuid().ToString("N");
        var mockSystemClock = new MockSystemClock(DateTimeOffset.Now);
        using var fasterKv = CreateKvStore(guid, mockSystemClock);
        
        var result = fasterKv.GetOrAdd(guid, _ => new Data()
        {
            One = "one",
            Two = 2
        }, TimeSpan.FromSeconds(1));
        
        Assert.Equal("one", result.One);
        Assert.Equal(2, result.Two);

        mockSystemClock.AddSeconds(2);

        result = fasterKv.GetOrAdd(guid, _ => new Data
        {
            One = "two",
            Two = 3
        }, TimeSpan.FromSeconds(1));

        Assert.Equal("two", result.One);
        Assert.Equal(3, result.Two);
    }
    
    // below test GetOrAddAsync
    
    [Fact]
    public async Task GetOrAddAsync_Should_Return_Existing_Value()
    {

        var guid = Guid.NewGuid().ToString("N");
        using var fasterKv = CreateKvStore(guid);
        
        var data = new Data
        {
            One = "one",
            Two = 2
        };
        fasterKv.Set(guid, data);

        var result = await fasterKv.GetOrAddAsync<Data>(guid, _ => Task.FromResult(new Data
        {
            One = "two",
            Two = 3
        }));

        Assert.Equal(data, result);
    }
    
    [Fact]
    public async Task GetOrAddAsync_Should_Return_New_Value()
    {
        var guid = Guid.NewGuid().ToString("N");
        using var fasterKv = CreateKvStore(guid);

        var result = await fasterKv.GetOrAddAsync<Data>(guid, (_) => Task.FromResult(new Data
        {
            One = "two",
            Two = 3
        }));

        Assert.Equal("two", result.One);
        Assert.Equal(3, result.Two);
    }
    
    [Fact]
    public async Task GetOrAddAsync_Should_Return_NewValue_When_Expired()
    {
        var guid = Guid.NewGuid().ToString("N");
        var mockSystemClock = new MockSystemClock(DateTimeOffset.Now);
        using var fasterKv = CreateKvStore(guid, mockSystemClock);

        var data = new Data
        {
            One = "one",
            Two = 2
        };
        fasterKv.Set(guid, data, TimeSpan.FromSeconds(1));
        
        mockSystemClock.AddSeconds(2);

        var result = await fasterKv.GetOrAddAsync(guid, _ => Task.FromResult(new Data
        {
            One = "two",
            Two = 3
        }));

        Assert.Equal("two", result.One);
        Assert.Equal(3, result.Two);
    }
    
    [Fact]
    public async Task GetOrAddAsync_Should_Return_NewValue_When_Expired_And_Refresh()
    {
        var guid = Guid.NewGuid().ToString("N");
        var mockSystemClock = new MockSystemClock(DateTimeOffset.Now);
        using var fasterKv = CreateKvStore(guid, mockSystemClock);
        
        var result = await fasterKv.GetOrAddAsync(guid, _ => Task.FromResult(new Data()
        {
            One = "one",
            Two = 2
        }), TimeSpan.FromSeconds(1));
        
        Assert.Equal("one", result.One);
        Assert.Equal(2, result.Two);

        mockSystemClock.AddSeconds(2);

        result = await fasterKv.GetOrAddAsync(guid, _ => Task.FromResult(new Data
        {
            One = "two",
            Two = 3
        }), TimeSpan.FromSeconds(1));

        Assert.Equal("two", result.One);
        Assert.Equal(3, result.Two);
    }
}

