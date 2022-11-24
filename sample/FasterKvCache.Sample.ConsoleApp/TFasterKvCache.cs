using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;

namespace FasterKvCache.Sample.ConsoleApp;

public class TFasterKvCache
{
    public async Task Run()
    {
        // create a FasterKvCache
        var cache = new FasterKvCache<string>("MyTCache",
            new DefaultSystemClock(),
            new FasterKvCacheOptions(),
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MyTCache"
                }
            },
            null);

        var key = Guid.NewGuid().ToString("N");

        // sync 
        // set key and value with expiry time
        cache.Set(key, "my cache sync", TimeSpan.FromMinutes(5));

        // get
        var result = cache.Get(key);
        Console.WriteLine(result);

        // delete
        cache.Delete(key);

        // async
        // set
        await cache.SetAsync(key, "my cache async");


        // get
        result = await cache.GetAsync(key);
        Console.WriteLine(result);

        // delete
        await cache.DeleteAsync(key);
    }
}