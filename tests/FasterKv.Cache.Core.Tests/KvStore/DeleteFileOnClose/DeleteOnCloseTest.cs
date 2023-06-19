using FasterKv.Cache.Core.Abstractions;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;

namespace FasterKv.Cache.Core.Tests.KvStore.DeleteFileOnClose;

public class DeleteOnCloseTest
{
    private string GetPath()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"./unit-test/faster-kv-store-delete-on-close-test/{guid}/log";
    }
    
    [Fact]
    public void Should_Not_Delete_On_Close()
    {
        var path = GetPath();
        var fasterKv = new FasterKvCache<string>(null!,
            new DefaultSystemClock(),
            new FasterKvCacheOptions
            {
                SerializerName = "MessagePack",
                ExpiryKeyScanInterval = TimeSpan.Zero,
                IndexCount = 16384,
                MemorySizeBit = 10,
                PageSizeBit = 10,
                ReadCacheMemorySizeBit = 10,
                ReadCachePageSizeBit = 10,
                PreallocateFile = false,
                DeleteFileOnClose = false,
                LogPath = path
            },
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MessagePack"
                }
            },
            null);
        
        fasterKv.Set("key", "value");

        Assert.Equal("value", fasterKv.Get("key"));
        
        fasterKv.Dispose();
        
        Assert.True(File.Exists($"{path}.log.0"));
        Assert.True(File.Exists($"{path}.obj.log.0"));
        
        Cleanup(path);
    }

    [Fact]
    public void Should_Restore_The_Data()
    {
        var path = GetPath();
        var fasterKv = new FasterKvCache<string>(null!,
            new DefaultSystemClock(),
            new FasterKvCacheOptions
            {
                SerializerName = "MessagePack",
                ExpiryKeyScanInterval = TimeSpan.Zero,
                IndexCount = 16384,
                MemorySizeBit = 10,
                PageSizeBit = 10,
                ReadCacheMemorySizeBit = 10,
                ReadCachePageSizeBit = 10,
                PreallocateFile = false,
                DeleteFileOnClose = false,
                LogPath = path
            },
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MessagePack"
                }
            },
            null);
         
        for (int i = 0; i < 100; i++)
        {
            fasterKv.Set($"key{i}", $"value{i}");   
        }

        Assert.Equal("value0", fasterKv.Get("key0"));
        
        fasterKv.Dispose();
        
        Assert.True(File.Exists($"{path}.log.0"));
        Assert.True(File.Exists($"{path}.obj.log.0"));
        
        fasterKv = new FasterKvCache<string>(null!,
            new DefaultSystemClock(),
            new FasterKvCacheOptions
            {
                SerializerName = "MessagePack",
                ExpiryKeyScanInterval = TimeSpan.Zero,
                IndexCount = 16384,
                MemorySizeBit = 10,
                PageSizeBit = 10,
                ReadCacheMemorySizeBit = 10,
                ReadCachePageSizeBit = 10,
                PreallocateFile = false,
                DeleteFileOnClose = false,
                TryRecoverLatest = true,
                LogPath = path
            },
            new IFasterKvCacheSerializer[]
            {
                new MessagePackFasterKvCacheSerializer
                {
                    Name = "MessagePack"
                }
            },
            null);
        
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal($"value{i}", fasterKv.Get($"key{i}"));
        }
        
        fasterKv.Dispose();
        
        Cleanup(path);
    }
    
    private static void Cleanup(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }
}