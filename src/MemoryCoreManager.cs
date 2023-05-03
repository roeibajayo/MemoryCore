using MemoryCore.KeyedLocker;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace MemoryCore;

internal sealed partial class MemoryCoreManager : IMemoryCore
{
    private const int clearInterval = 20 * 1000;

    internal readonly ConcurrentDictionary<string, MemoryEntry> entries;
    internal readonly KeyedLocker<string> keyedLocker = new();
    internal IDateTimeOffsetProvider dateTimeOffsetProvider = new DateTimeOffsetProvider();
    internal readonly Timer timer;

    public MemoryCoreManager() : this(StringComparison.Ordinal) { }
    public MemoryCoreManager(StringComparison stringComparison)
    {
        entries = new(comparer: StringComparer.FromComparison(stringComparison));
        timer = new((state) => ClearExpired(), null, clearInterval, clearInterval);
    }

    public void Add<T>(string key, T value, TimeSpan absoluteExpiration, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var now = dateTimeOffsetProvider.Now;
        var expiration = now + (long)absoluteExpiration.TotalMilliseconds;
        entries[key] = new MemoryEntry
        {
            Key = key,
            Value = value,
            Tags = tags,
            AbsoluteExpiration = expiration
        };
    }

    public void AddSliding<T>(string key, T value, TimeSpan duration, TimeSpan? absoluteExpiration = null, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        var now = dateTimeOffsetProvider.Now;
        var expiration = absoluteExpiration is null ? default : (now + (long)absoluteExpiration.Value.TotalMilliseconds);
        var entry = new MemoryEntry
        {
            Key = key,
            Value = value,
            Tags = tags,
            AbsoluteExpiration = expiration,
            SlidingExpiration = (long)duration.TotalMilliseconds
        };
        entries[key] = entry;
        entry.Touch(now);
    }

    public bool Exists(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        return TryGet(key, out _);
    }

    public IEnumerable<string> GetKeys()
    {
        var entries = this.entries.ToArray();
        var now = dateTimeOffsetProvider.Now;
        foreach (var entry in entries)
        {
            if (!entry.Value.IsExpired(now))
                yield return entry.Key;
        }
    }

    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        entries.TryRemove(key, out _);
    }

    public bool TryGet<T>(string key, out T? item)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (TryGet(key, out object value))
        {
            item = (T?)value;
            return true;
        }

        item = default;
        return false;
    }
    internal bool TryGet(string key, out object? item)
    {
        if (entries.TryGetValue(key, out var entry))
        {
            var now = dateTimeOffsetProvider.Now;

            if (entry.IsExpired(now))
            {
                Remove(key);
                item = default;
                return false;
            }

            item = entry.Value;
            entry.Touch(now);
            return true;
        }

        item = default;
        return false;
    }

    public T? TryGetOrSet<T>(string key, Func<T> getValueFunction, TimeSpan absoluteExpiration, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            using var forcesetLocker = keyedLocker.Lock(key);
            if (forcesetLocker.locked)
            {
                forcesetLocker.Dispose();
                return TryGetOrSet(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
            }

            var value = getValueFunction();
            Add(key, value, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;


        using var locker = keyedLocker.Lock(key);
        if (locker.locked)
        {
            locker.Dispose();
            return TryGetOrSet(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
        }

        item = getValueFunction();

        if (!allowDefault && item is null)
            return item;

        Add(key, item, absoluteExpiration, tags);
        return item;
    }
    public async Task<T?> TryGetOrSetAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan absoluteExpiration, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            using var forcesetLocker = keyedLocker.Lock(key);
            if (forcesetLocker.locked)
            {
                forcesetLocker.Dispose();
                return await TryGetOrSetAsync(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
            }

            var value = await getValueFunction();
            Add(key, value, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;

        using var locker = keyedLocker.Lock(key);
        if (locker.locked)
        {
            locker.Dispose();
            return await TryGetOrSetAsync(key, getValueFunction, absoluteExpiration, allowDefault, forceSet, tags);
        }

        item = await getValueFunction();

        if (!allowDefault && item is null)
            return item;

        Add(key, item, absoluteExpiration, tags);
        return item;
    }
    public T? TryGetOrSetSliding<T>(string key, Func<T> getValueFunction, TimeSpan duration, TimeSpan? absoluteExpiration = null, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            var value = getValueFunction();
            AddSliding(key, value, duration, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;

        item = getValueFunction();

        if (!allowDefault && item is null)
            return item;

        AddSliding(key, item, duration, absoluteExpiration, tags);
        return item;
    }
    public async Task<T?> TryGetOrSetSlidingAsync<T>(string key, Func<Task<T>> getValueFunction, TimeSpan duration, TimeSpan? absoluteExpiration = null, bool allowDefault = false, bool forceSet = false, params string[] tags)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (forceSet)
        {
            var value = await getValueFunction();
            AddSliding(key, value, duration, absoluteExpiration, tags);
            return value;
        }

        if (TryGet(key, out T item))
            return item;

        item = await getValueFunction();

        if (!allowDefault && item is null)
            return item;

        AddSliding(key, item, duration, absoluteExpiration, tags);
        return item;
    }

    public int Count() =>
        entries.Count;

    internal void ClearExpired()
    {
        var now = dateTimeOffsetProvider.Now;
        var removedKeys = new List<string>();

        foreach (var entry in entries.Values)
        {
            if (entry.IsExpired(now))
                removedKeys.Add(entry.Key);
        }

        foreach (var key in removedKeys)
            entries.TryRemove(key, out _);
    }

    public void Clear() =>
        entries.Clear();


    //IMemoryCache:
    public bool TryGetValue(object key, out object? value)
    {
        return TryGet(key.ToString(), out value);
    }
    public ICacheEntry CreateEntry(object key)
    {
        return new MemoryEntry
        {
            Key = key.ToString()
        };
    }
    public void Remove(object key)
    {
        Remove(key.ToString());
    }
    public void Dispose()
    {
        timer.Dispose();
    }
}
