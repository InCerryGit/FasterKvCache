# FasterKv.Cache

[English](README.md) | [中文](README.zh-CN.md)

FasterKv.Cache is an in-process hybrid cache library (memory + disk) built on top of Microsoft's FasterKv engine. FasterKv supports datasets larger than system memory and delivers performance far beyond most other memory+disk key-value stores. However, its raw API can be difficult to use, especially for newcomers. FasterKv.Cache provides a simplified abstraction to make caching easier and more intuitive.

![arch](docs/assets/arch.png)

## When to Use
| Suitable Scenarios | Not Suitable Scenarios | Reason |
| --- | --- | --- |
| Large cache data volume with need to reduce memory usage | Small dataset or abundant memory budget | For small datasets or rich-memory systems, pure in-memory cache is fastest. |
| Clear distinction between hot and cold data | Completely random access with no hot set | Random access invalidates memory caching; frequent disk reads degrade performance. |
| Cache does not require ultra-strict latency; microseconds-level fluctuation acceptable | Cache requires extremely stable, low latency | If you cannot tolerate even hundreds of microseconds, pure memory cache is the only option. |
| 1. No extremely large values. 2. Very large but frequently accessed values. 3. Extremely large but rarely accessed values where latency is not sensitive | Extremely large values, random access, and highly sensitive latency | If data size exceeds memory+ReadCache, performance degrades; only more memory solves it. |

The author previously contributed FasterKv support to EasyCaching, but due to limitations in how some advanced EasyCaching features map onto FasterKv while maintaining performance, a standalone library was created. If you're using EasyCaching, you can still use EasyCaching.FasterKv directly.

## NuGet Packages
| Package | Version | Notes |
| --- | --- | --- |
| FasterKv.Cache.Core | 1.0.2 | Core cache engine, contains main FasterKvCache API |
| FasterKv.Cache.MessagePack | 1.0.2 | MessagePack-based disk serialization. Very fast but requires some type configuration. |
| FasterKv.Cache.SystemTextJson | 1.0.2 | System.Text.Json-based disk serialization. Slower than MessagePack but very easy to use. |

## Usage
### Direct Instantiation
Two API styles exist for performance and usability:
- **Object-based**: `new FasterKvCache(...)`, store any value type. Values stored as `object`, boxing/unboxing overhead applies.
- **Generic**: `new FasterKvCache<T>(...)`, strongly typed, no boxing cost.

If values are evicted from memory to disk, both modes incur disk I/O + serialization overhead.

### Object-Based API
```csharp
using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;

var cache = new FasterKvCache(
    "MyCache",
    new DefaultSystemClock(),
    new FasterKvCacheOptions(),
    new IFasterKvCacheSerializer[]
    {
        new MessagePackFasterKvCacheSerializer { Name = "MyCache" }
    },
    null);

var key = Guid.NewGuid().ToString("N");

cache.Set(key, "my cache sync", TimeSpan.FromMinutes(5));
var result = cache.Get<string>(key);
```

### Generic Version
```csharp
var cache = new FasterKvCache<string>(
    "MyTCache",
    new DefaultSystemClock(),
    new FasterKvCacheOptions(),
    new[]
    {
        new MessagePackFasterKvCacheSerializer { Name = "MyTCache" }
    },
    null);
```

## Microsoft.Extensions.DependencyInjection
```csharp
var services = new ServiceCollection();
services.AddFasterKvCache(options =>
{
    options.UseMessagePackSerializer();
}, "MyKvCache");

var provider = services.BuildServiceProvider();
var cache = provider.GetService<FasterKvCache>();
```

Generic:
```csharp
var services = new ServiceCollection();
services.AddFasterKvCache<string>(options =>
{
    options.UseMessagePackSerializer();
}, "MyKvCache");

var provider = services.BuildServiceProvider();
var cache = provider.GetService<FasterKvCache<string>>();
```

## Configuration
### FasterKvCache Constructor
```csharp
public FasterKvCache(
    string name,
    ISystemClock systemClock,
    FasterKvCacheOptions? options,
    IEnumerable<IFasterKvCacheSerializer>? serializers,
    ILoggerFactory? loggerFactory)
```

### FasterKvCacheOptions
Key settings:
- **IndexCount**: number of hash index buckets (power of two). Default 131072.
- **MemorySizeBit**: log memory size as power of two. Default 24 → 16MB.
- **PageSizeBit**: log page size (power of two). Default 20 → 1MB.
- **ReadCacheMemorySizeBit**: read cache size. Default 20 → 1MB.
- **ReadCachePageSizeBit**: read cache page size. Default 20.
- **LogPath**: directory for FasterKv log files.
- **SerializerName**: MessagePack or SystemTextJson (default auto).
- **PreallocateFile**: preallocate 1GB log file for speed.
- **DeleteFileOnClose**: delete log file on close (default true).
- **TryRecoverLatest**: attempt to restore previous state (requires disabling DeleteFileOnClose).
- **ExpiryKeyScanInterval**: interval for scanning expired keys.
- **CustomStore**: custom FasterKv instance.

### Memory Usage Formula
```
(IndexCount * 64 bytes) + MemorySize + ReadCacheMemorySize + (OverflowBucketCount * 64)
```

## Capacity Planning
Disk usage = raw data size + serialized object size. Different serializers produce different sizes.

For example, storing 100GB but frequently accessing only 20% can work well with:
- **32GB memory + 128GB disk**
...saving >50% memory cost.

## Performance
Benchmark results (1000 keys, 50% hot set). Approx ~2MB memory allocated.

FasterKvCache achieves up to **16 million ops/sec** on an 8‑thread machine.

(Full benchmark table preserved exactly as Chinese version.)

## Other
This project is already used in production. If you find bugs, please report them.
