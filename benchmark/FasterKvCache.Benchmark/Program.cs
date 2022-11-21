using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using FasterKv.Cache.Core;
using FasterKv.Cache.Core.Configurations;
using FasterKv.Cache.MessagePack;
using Microsoft.Extensions.DependencyInjection;

BenchmarkDotNet.Running.BenchmarkRunner.Run<FasterKvBenchmark>();

public enum TestType
{
    Read,
    Write,
    Random
}

[GcForce]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(RuntimeMoniker.Net60, launchCount: 1, warmupCount: 5, targetCount: 10)]
[MemoryDiagnoser]
#nullable disable
public class FasterKvBenchmark
{
    private const long Count = 1000;
    private static readonly Random _random = new Random(1024);
    private FasterKvCache<string> _provider;
    private static readonly TimeSpan _default = TimeSpan.FromSeconds(30);
    
    
    [Params(TestType.Read, TestType.Write, TestType.Random)]
    public TestType Type { get; set; }
    
    [Params(1,4,8)]
    public int ThreadCount { get; set; }

    private static readonly string[] HotKeys = Enumerable.Range(0, (int)(Count * 0.5))
        .Select(i => $"cache_{_random.Next(0, (int) Count)}")
        .ToArray();
    
    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddFasterKvCache<string>(options =>
        {
            options.IndexCount = 1024;
            options.MemorySizeBit = 20;
            options.PageSizeBit = 20;
            options.ReadCacheMemorySizeBit = 20;
            options.ReadCachePageSizeBit = 20;
            // use MessagePack serializer
            options.UseMessagePackSerializer();
        }, "MyKvCache");
        IServiceProvider serviceProvider = services.BuildServiceProvider();
        _provider =  serviceProvider.GetService<FasterKvCache<string>>()!;

        switch (Type)
        {
            case TestType.Write:
                break;
            case TestType.Read:
            case TestType.Random:
                for (int i = 0; i < Count; i++)
                {
                    _provider!.Set($"cache_{i}", "cache", _default);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    [Benchmark]
    public async Task Full()
    {
        var tasks = new Task[ThreadCount];
        var threadOpCount = (int)(HotKeys.Length / ThreadCount);
        for (int i = 0; i < ThreadCount; i++)
        {
            int i1 = i;
            tasks[i] = Task.Run(() =>
            {
                var j = i1 * threadOpCount;
                switch (Type)
                {
                    case TestType.Read:
                        for (; j < threadOpCount; j++)
                        {
                            _provider.Get(HotKeys[j]);
                        }

                        break;
                    case TestType.Write:
                        for (; j < threadOpCount; j++)
                        {
                            _provider.Set(HotKeys[j], "cache", _default);
                        }

                        break;
                    case TestType.Random:
                        for (; j < threadOpCount; j++)
                        {
                            if (j % 2 == 0)
                            {
                                _provider.Get(HotKeys[j]);
                            }
                            else
                            {
                                _provider.Set(HotKeys[j], "cache", _default);
                            }
                        }

                        break;
                }
            });
        }

        await Task.WhenAll(tasks);
    }


    [GlobalCleanup]
    public void Cleanup()
    {
        _provider.Dispose();
    }
}