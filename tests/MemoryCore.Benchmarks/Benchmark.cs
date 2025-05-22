using BenchmarkDotNet.Attributes;
using BitFaster.Caching.Lru;
using Microsoft.Extensions.Caching.Memory;

namespace MemoryCore.Benchmarks;

[MemoryDiagnoser(false)]
[ShortRunJob]
[MarkdownExporterAttribute.GitHub]
public class Benchmark
{
    private MemoryCache? memoryCache;
    private MemoryCoreManager? memoryCore;
    private ConcurrentLru<string, object>? lur;

    [GlobalSetup]
    public void Setup()
    {
        memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCore = new MemoryCoreManager();
        lur = new ConcurrentLru<string, object>(128);

        memoryCore.Add("global-key", "value", TimeSpan.FromMinutes(10));
        memoryCache.Set("global-key", "value", DateTimeOffset.Now.AddMinutes(10));
        lur.AddOrUpdate("global-key", "value");
    }

    [Benchmark]
    public void MemoryCore_Add()
    {
        memoryCore!.Add("key-for-add", "value", TimeSpan.FromMinutes(1));
    }
    [Benchmark]
    public string MemoryCache_Add()
    {
        return memoryCache!.Set("key-for-add", "value", DateTimeOffset.Now.AddMinutes(1));
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
        return (string)memoryCache!.Get("global-key")!;
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
        return memoryCache!.TryGetValue("global-key", out _);
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
    public void MemoryCache_Remove()
    {
        memoryCache!.Remove("global-key");
    }
    [Benchmark]
    public bool ConcurrentLru_Remove()
    {
        return lur!.TryRemove("global-key");
    }
}
