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

## Benchmarks MemoryCore (1.6.3) vs Microsoft.Extensions.Caching.Memory (9.0.5) on .NET9:

| Method             | Mean      | Allocated | Speedup |
|------------------- |----------:|----------:|--------:|
| MemoryCore_Add     |  35.76 ns |      80 B |    4.8x |
| Microsoft_Add      | 170.81 ns |     104 B | 	       |
|                    |           |           | 	       | 
| MemoryCore_Get     |  10.91 ns |         - | 	  2.4x |  
| Microsoft_Get      |  26.39 ns |         - | 	       | 
|                    |           |           | 	       | 
| MemoryCore_Exists  |  10.98 ns |         - | 	  2.4x | 
| Microsoft_Exists   |  26.47 ns |         - | 	       | 
|                    |           |           | 	       | 
| MemoryCore_Remove  |  18.50 ns |         - | 	  2.2x | 
| Microsoft_Remove   |  41.52 ns |         - | 	       | 

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
