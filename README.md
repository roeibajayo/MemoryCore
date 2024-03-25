# MemoryCore

[![NuGet](https://img.shields.io/nuget/dt/MemoryCore.svg)](https://www.nuget.org/packages/MemoryCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MemoryCore.svg)](https://www.nuget.org/packages/MemoryCore)

High-performance yet easy to use memory manager implementation in .NET.

## 🚀 Features
✔ Super FAST and low memory usage. 🔥

✔ Support for joint execution for GetOrSetAsync methods, so only 1 runs concurrently. 🔥

✔ Support for IMemoryCahce intrerface.

✔ Dependency Injection ready.

✔ Support for tags.

✔ Support for keyless items.

✔ Support for persistent items.

✔ Developers friendly ❤️ Easy to use.

## Benchmarks MemoryCore (1.5.0) vs System.Runtime.Caching (8.0.0):

|             Method |      Mean |    Error |   StdDev | Allocated |
|------------------- |----------:|---------:|---------:|----------:|
|     MemoryCore_Add |  52.32 ns | 1.036 ns | 1.383 ns |      80 B |
|    MemoryCache_Add | 332.24 ns | 6.186 ns | 5.786 ns |     272 B |
|                                                                  |
|     MemoryCore_Get |  22.03 ns | 0.484 ns | 0.595 ns |         - |
|    MemoryCache_Get |  98.35 ns | 1.961 ns | 2.685 ns |      32 B |
|                                                                  |
|  MemoryCore_Exists |  21.95 ns | 0.376 ns | 0.334 ns |         - |
| MemoryCache_Exists | 329.76 ns | 5.193 ns | 4.858 ns |     752 B |

## Install & Registering:

Install [MemoryCore with NuGet](https://www.nuget.org/packages/MemoryCore):

    Install-Package MemoryCore
    
Or via the .NET Core command line interface:

    dotnet add package MemoryCore
    
then register the required services easly:

```csharp
services.AddMemoryCore();
```

or create an instance:

```csharp
using var cache = new MemoryCoreManager();
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
    
    public string GetOrAddItem()
    {
        return cache.TryGetOrAdd("key", 
		    () => "value", 
		    TimeSpan.FromMinutes(1));
    }
    
    public async Task<string> GetOrAddItemAsync()
    {
        return await cache.TryGetOrAddAsync("key", 
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
