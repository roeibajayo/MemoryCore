using Microsoft.Extensions.Caching.Memory;

namespace MemoryCore;

public sealed partial class MemoryCoreManager : IMemoryCore
{
    public bool TryGetValue(object key, out object? value)
    {
        return TryGet(key.ToString()!, out value);
    }

    public ICacheEntry CreateEntry(object key)
    {
        return new MemoryEntry
        {
            Key = key.ToString()!
        };
    }

    public void Remove(object key)
    {
        Remove(key.ToString()!);
    }

    public void Dispose()
    {
        entries.Clear();
        timer.Dispose();
    }
}
