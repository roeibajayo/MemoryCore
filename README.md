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
|     MemoryCore_Add |  51.19 ns | 1.044 ns |  2.356 ns |      72 B |
|    MemoryCache_Add | 338.31 ns | 6.778 ns | 15.710 ns |     272 B |
|                                                                   |
|     MemoryCore_Get |  22.06 ns | 0.462 ns |  0.901 ns |         - |
|    MemoryCache_Get |  90.27 ns | 1.673 ns |  3.380 ns |      32 B |
|                                                                   |
|  MemoryCore_Exists |  22.38 ns | 0.466 ns |  0.622 ns |         - |
| MemoryCache_Exists | 348.40 ns | 6.873 ns | 14.194 ns |     752 B |

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
        return cache.TryGetOrSet("key", 
		    () => "value", 
		    TimeSpan.FromMinutes(1));
    }
    
    public async Task<string> GetOrSetItemAsync()
    {
        return await cache.TryGetOrSetAsync("key", 
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
