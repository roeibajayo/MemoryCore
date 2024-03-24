using System.Collections.Concurrent;

namespace MemoryCore.KeyedLocker;

/// <summary>
/// Represents a list of lockers that can be used to lock threads based on a key.
/// </summary>
/// <typeparam name="TKey"></typeparam>
internal sealed class KeyedLocker<TKey> where TKey : notnull
{
    internal readonly ConcurrentDictionary<TKey, LockerItem<TKey>> lockers = [];

    private LockerItem<TKey> GetOrCreateItem(TKey key)
    {
        if (lockers.TryGetValue(key, out var item))
        {
            item.Counter.IncrementCount();
            return item;
        }

        item = new(key, new LockerCounter<TKey>());
        lockers[key] = item;
        return item;
    }

    internal bool IsInUse(TKey key)
    {
        return lockers.ContainsKey(key);
    }

    internal Releaser<TKey> Lock(TKey key)
    {
        var item = GetOrCreateItem(key);
        var locked = false;
        if (item.Counter.Count != 1)
        {
            item.Counter.Semaphore!.Wait();
            locked = true;
        }
        return new Releaser<TKey>(lockers, item, locked);
    }

    internal async Task<Releaser<TKey>> LockAsync(TKey key, CancellationToken cancellationToken)
    {
        var item = GetOrCreateItem(key);
        var locked = false;
        if (item.Counter.Count != 1)
        {
            await item.Counter.Semaphore!.WaitAsync(cancellationToken);
            locked = true;
        }
        return new Releaser<TKey>(lockers, item, locked);
    }
}
