# MemoryCore

[![NuGet](https://img.shields.io/nuget/dt/MemoryCore.svg)](https://www.nuget.org/packages/MemoryCore) 
[![NuGet](https://img.shields.io/nuget/vpre/MemoryCore.svg)](https://www.nuget.org/packages/MemoryCore)

High-performance yet easy to use memory manager implementation in .NET.

## 🚀 Features
✔ Super FAST and low memory usage. 🔥

✔ Support for joint execution for GetOrSetAsync methods, so only 1 runs concurrently. 🔥

✔ Support for IMemoryCache interface.

✔ Dependency Injection ready.

✔ Support for tags.

✔ Support for keyless items.

✔ Support for persistent items.

✔ Developers friendly ❤️ Easy to use.

## Benchmarks MemoryCore (1.5.0) vs System.Runtime.Caching (8.0.0):

| Method             | Mean      | Error    | StdDev   | Allocated |
|------------------- |----------:|---------:|---------:|----------:|
| MemoryCore_Add     |  53.59 ns | 0.992 ns | 1.887 ns |      80 B |
| MemoryCache_Add    | 321.22 ns | 2.066 ns | 1.831 ns |     272 B |
|                    |           |          |          |           |
| MemoryCore_Get     |  21.14 ns | 0.289 ns | 0.270 ns |         - |
| MemoryCache_Get    |  85.09 ns | 1.751 ns | 2.621 ns |      32 B |
|                    |           |          |          |           |
| MemoryCore_Exists  |  20.99 ns | 0.268 ns | 0.251 ns |         - |
| MemoryCache_Exists | 340.56 ns | 6.661 ns | 6.840 ns |     752 B |

## Install & Registering:

Install [MemoryCore with NuGet](https://www.nuget.org/packages/MemoryCore):

    Install-Package MemoryCore
    
Or via the .NET Core command line interface:

    dotnet add package MemoryCore
    
then register the required services easily:

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
