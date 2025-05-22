using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Caching.Memory;

namespace MemoryCore.Benchmarks;

[MemoryDiagnoser(false)]
[ShortRunJob]
[MarkdownExporterAttribute.GitHub]
public class Benchmark
{
    private MemoryCache? memoryCache;
    private MemoryCoreManager? memoryCore;

    [GlobalSetup]
    public void Setup()
    {
        memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCore = new MemoryCoreManager();

        memoryCore.Add("global-key", "value", TimeSpan.FromMinutes(10));
        memoryCache.Set("global-key", "value", DateTimeOffset.Now.AddMinutes(10));
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
    public string MemoryCore_Get()
    {
        _ = memoryCore!.TryGet<string>("global-key", out var value);
        return value!;
    }
    [Benchmark]
    public string MemoryCache_Get()
    {
        return (string)memoryCache!.Get("global-key")!;
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
    public void MemoryCore_Remove()
    {
        memoryCore!.Remove("global-key");
    }
    [Benchmark]
    public void MemoryCache_Remove()
    {
        memoryCache!.Remove("global-key");
    }
}
