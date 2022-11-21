using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;

namespace FasterKvCache.Sample.ConsoleApp;

public class ObjectFasterKvCache
{
    public async Task Run()
    {
        // create a FasterKvCache
        var cache = new FasterKv.Cache.Core.FasterKvCache("MyCache",
            new DefaultSystemClock(),
            new FasterKvCacheOptions(),
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MyCache"
                }
            },
            null);

        var key = Guid.NewGuid().ToString("N");

        // sync 
        // set key and value with expiry time
        cache.Set(key, "my cache sync", TimeSpan.FromMinutes(5));

        // get
        var result = cache.Get<string>(key);
        Console.WriteLine(result);

        // delete
        cache.Delete(key);

        // async
        // set
        await cache.SetAsync(key, "my cache async");
        
        // get
        result = await cache.GetAsync<string>(key);
        Console.WriteLine(result);

        // delete
        await cache.DeleteAsync(key);
        
        // set other type object
        cache.Set(key, new DateTime(2022,2,22));
        Console.WriteLine(cache.Get<DateTime>(key));
    }
}