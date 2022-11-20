using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.Core.Tests.KvStore;
using FasterKv.Cache.MessagePack;
using FasterKv.Cache.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;

namespace FasterKv.Cache.Core.Tests.DependencyInjection;

public class FasterKvCacheDiTest
{
    [Fact]
    public void Use_MessagePackSerializer_Create_FasterKvCache_Should_Success()
    {
        var services = new ServiceCollection();
        services.AddFasterKvCache(options =>
        {
            options.UseMessagePackSerializer();
        },"MyKvCache");
        var provider = services.BuildServiceProvider();

        var cache = provider.GetService<FasterKvCache>();
        Assert.NotNull(cache);
        
        cache!.Set("abc","abc");
        var result = cache.Get<string>("abc");
        Assert.Equal("abc", result);
    }
    
    [Fact]
    public void Use_MessagePackSerializer_Create_FasterKvCacheTValue_Should_Success()
    {
        var services = new ServiceCollection();
        services.AddFasterKvCache<Data>(options =>
        {
            options.UseMessagePackSerializer();
        },"MyKvCache");
        var provider = services.BuildServiceProvider();

        var cache = provider.GetService<FasterKvCache<Data>>();
        Assert.NotNull(cache);

        var data = new Data
        {
            One = "1024",
            Two = 1024
        };
        cache!.Set("abc",data);
        var result = cache.Get("abc");
        Assert.Equal(data, result);
    }    [Fact]
    public void Use_SystemTextJson_Create_FasterKvCache_Should_Success()
    {
        var services = new ServiceCollection();
        services.AddFasterKvCache(options =>
        {
            options.UseSystemTextJsonSerializer();
        },"MyKvCache");
        var provider = services.BuildServiceProvider();

        var cache = provider.GetService<FasterKvCache>();
        Assert.NotNull(cache);
        
        cache!.Set("abc","abc");
        var result = cache.Get<string>("abc");
        Assert.Equal("abc", result);
    }
    
    [Fact]
    public void Use_SystemTextJson_Create_FasterKvCacheTValue_Should_Success()
    {
        var services = new ServiceCollection();
        services.AddFasterKvCache<Data>(options =>
        {
            options.UseSystemTextJsonSerializer();
        },"MyKvCache");
        var provider = services.BuildServiceProvider();

        var cache = provider.GetService<FasterKvCache<Data>>();
        Assert.NotNull(cache);

        var data = new Data
        {
            One = "1024",
            Two = 1024
        };
        cache!.Set("abc",data);
        var result = cache.Get("abc");
        Assert.Equal(data, result);
    }
}