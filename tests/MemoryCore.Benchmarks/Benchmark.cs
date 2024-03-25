using BenchmarkDotNet.Attributes;
using System.Runtime.Caching;

namespace MemoryCore.Benchmarks;

[MemoryDiagnoser(false)]
public class Benchmark
{
    private MemoryCache? memoryCache;
    private MemoryCoreManager? memoryCore;

    [GlobalSetup]
    public void Setup()
    {
        memoryCache = new MemoryCache("global");
        memoryCore = new MemoryCoreManager();

        memoryCore.Add("global-key", "value", TimeSpan.FromMinutes(10));
        memoryCache.Add("global-key", "value", DateTimeOffset.Now.AddMinutes(10));
    }

    [Benchmark]
    public void MemoryCore_Add()
    {
        memoryCore!.Add("key-for-add", "value", TimeSpan.FromMinutes(1));
    }

    [Benchmark]
    public void MemoryCache_Add()
    {
        memoryCache!.Add("key-for-add", "value", DateTimeOffset.Now.AddMinutes(1));
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
    public bool MemoryCore_Exists()
    {
        return memoryCore!.Exists("global-key");
    }

    [Benchmark]
    public bool MemoryCache_Exists()
    {
        return memoryCache!.Any(x => x.Key == "global-key");
    }
}
