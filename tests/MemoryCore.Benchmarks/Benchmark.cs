using BenchmarkDotNet.Attributes;
using BitFaster.Caching.Lru;
using System.Runtime.Caching;

namespace MemoryCore.Benchmarks;

[MemoryDiagnoser(false)]
[RankColumn]
[ShortRunJob]
[MinColumn, MaxColumn, MedianColumn]
[MarkdownExporterAttribute.GitHub]
public class Benchmark
{
    private MemoryCache? memoryCache;
    private MemoryCoreManager? memoryCore;
    private ConcurrentLru<string, object>? lur;

    [GlobalSetup]
    public void Setup()
    {
        memoryCache = new MemoryCache("global");
        memoryCore = new MemoryCoreManager();
        lur = new ConcurrentLru<string, object>(128);

        memoryCore.Add("global-key", "value", TimeSpan.FromMinutes(10));
        memoryCache.Add("global-key", "value", DateTimeOffset.Now.AddMinutes(10));
        lur.AddOrUpdate("global-key", "value");
    }

    [Benchmark]
    public void MemoryCore_Add()
    {
        memoryCore!.Add("key-for-add", "value", TimeSpan.FromMinutes(1));
    }
    [Benchmark]
    public bool MemoryCache_Add()
    {
        return memoryCache!.Add("key-for-add", "value", DateTimeOffset.Now.AddMinutes(1));
    }
    [Benchmark]
    public void ConcurrentLru_Add()
    {
        lur!.AddOrUpdate("key-for-add", "value");
    }

    [Benchmark]
    public string? MemoryCore_Get()
    {
        return memoryCore!.Get<string>("global-key");
    }
    [Benchmark]
    public string MemoryCache_Get()
    {
        return (string)memoryCache!.Get("global-key");
    }
    [Benchmark]
    public string? ConcurrentLru_Get()
    {
        _ = lur!.TryGet("global-key", out var value);
        return (string)value!;
    }


    [Benchmark]
    public bool MemoryCore_Exists()
    {
        return memoryCore!.Exists("global-key");
    }
    [Benchmark]
    public bool MemoryCache_Exists()
    {
        return memoryCache!.Any(x => x.Key == "global-key");
    }
    [Benchmark]
    public bool ConcurrentLru_Exists()
    {
        return lur!.TryGet("global-key", out _);
    }

    [Benchmark]
    public void MemoryCore_Remove()
    {
        memoryCore!.Remove("global-key");
    }
    [Benchmark]
    public object MemoryCache_Remove()
    {
        return memoryCache!.Remove("global-key");
    }
    [Benchmark]
    public bool ConcurrentLru_Remove()
    {
        return lur!.TryRemove("global-key");
    }
}
