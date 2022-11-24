using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;

namespace FasterKv.Cache.Core.Tests.KvStore;

public class FasterKvStoreTestExpiry
{
    private FasterKvCache<Data> _fasterKv;

    private readonly Data _data = new()
    {
        One = "one",
        Two = 2
    };

    public FasterKvStoreTestExpiry()
    {
        _fasterKv = CreateKvStore();
    }

    private static FasterKvCache<Data> CreateKvStore()
    {
        return new FasterKvCache<Data>(null!,
            new DefaultSystemClock(),
            new FasterKvCacheOptions
            {
                IndexCount = 16384,
                MemorySizeBit = 10,
                PageSizeBit = 10,
                ReadCacheMemorySizeBit = 10,
                ReadCachePageSizeBit = 10,
                SerializerName = "MessagePack",
                ExpiryKeyScanInterval = TimeSpan.FromSeconds(1),
                LogPath = "./unit-test/faster-kv-store-expiry-test"
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
    public async Task Set_Key_With_Expired_Should_Return_Null()
    {
        var guid = Guid.NewGuid().ToString("N");
        _fasterKv.Set(guid, _data, TimeSpan.FromSeconds(1));

        await Task.Delay(2000);
        var result = _fasterKv.Get(guid);
        
        Assert.Null(result);
    }
    
    [Fact]
    public async Task ExpiryScanLoop_Should_Delete_Expiry_Key()
    {
        var guid = Guid.NewGuid().ToString("N");
        _fasterKv.Set(guid, _data, TimeSpan.FromSeconds(1));
        var result = _fasterKv.Get(guid);
        Assert.Equal(_data, result);
        
        await Task.Delay(3000);
        var wrapper = _fasterKv.GetWithOutExpiry(guid);
        
        Assert.Null(wrapper.Data);
    }
    
}
