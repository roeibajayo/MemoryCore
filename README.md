# MemoryCore

[![NuGet](https://img.shields.io/nuget/dt/MemoryCore.svg)](https://www.nuget.org/packages/MemoryCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MemoryCore.svg)](https://www.nuget.org/packages/MemoryCore)

High-performance yet easy to use memory manager implementation in .NET.

## 🚀 Features
✔ Super FAST and low memory usage. 🔥

✔ Support for joint execution for GetOrSet methods, so only 1 runs concurrently. 🔥

✔ Support for IMemoryCahce intrerface.

✔ Support for tags.

✔ Support for secured keys.

✔ Developers friendly ❤️ Easy to use.

## Benchmarks MemoryCore (1.0.0) vs System.Runtime.Caching (7.0.0):

|             Method |      Mean |    Error |    StdDev | Allocated |
|------------------- |----------:|---------:|----------:|----------:|
|     MemoryCore_Add |  63.86 ns | 1.259 ns |  3.651 ns |     112 B |
|    MemoryCache_Add | 343.80 ns | 6.870 ns | 15.080 ns |     272 B |
|                                                                   |
|     MemoryCore_Get |  33.91 ns | 0.699 ns |  0.883 ns |         - |
|    MemoryCache_Get |  91.22 ns | 1.743 ns |  2.007 ns |      32 B |
|                                                                   |
|  MemoryCore_Exists |  26.05 ns | 0.536 ns |  0.475 ns |         - |
| MemoryCache_Exists | 353.29 ns | 7.055 ns | 15.187 ns |     752 B |

## Install & Registering:

Install [MemoryCore with NuGet](https://www.nuget.org/packages/MemoryCore):

    Install-Package MemoryCore
    
Or via the .NET Core command line interface:

    dotnet add package MemoryCore

then register the required services easly:

```csharp
services.AddMemoryCore();
```

## Example of use:

```csharp
public class Example
{
    private readonly IMemoryCore cache;

    public Example(IMemoryCore cache)
    {
        this.cache = cache;
    }
    
    public void AddCacheItem()
    {
        cache.Add("key", "value", TimeSpan.FromMinutes(1));
    }
    
    public string GetCacheItem()
    {
        return cache.Get<string>("key");
    }    
    
    public string GetOrSetItem()
    {
        return cache.GetOrSet("key", 
		() => "value", 
		TimeSpan.FromMinutes(1));
    }
    
    public async Task<string> GetOrSetItemAsync()
    {
        return await cache.GetOrSetAsync("key", 
		async () => await GetValueAsync(), 
		TimeSpan.FromMinutes(1));
    }

    private async Task<string> GetValueAsync()
    {
        await Task.Delay(1000); //simulate action
	return "value";
    }
}
```

## Contribute
Please feel free to PR. I highly appreciate any contribution!
